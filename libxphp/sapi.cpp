#include "sapi.h"
#include <algorithm>

#define SAPI_MAX_HEADER_LENGTH 1024


sapi_module_struct sapi_warp_module = {
	(char*)"fastcgi",              // name
	(char*)"AspNetCore",           // pretty name
	sapi_php_startup,              // startup
	php_module_shutdown_wrapper,   // shutdown
	nullptr,                       // activate
	nullptr,                       // deactivate
	sapi_ub_write,                 // unbuffered write
	sapi_flush,                    // flush 
	nullptr,                       // get uid
	nullptr,                       // getenv
	php_error,                     // error handler
	nullptr,                       // header handler
	sapi_send_headers,             // send headers handler
	nullptr,                       // send header handler
	sapi_read_post,                // read POST data
	sapi_read_cookies,             // read Cookies
	sapi_php_register_variables,   // register server variables
	sapi_php_log_message,          // Log message
	nullptr,                       // Request Time
	nullptr,                       // Child Terminate
	STANDARD_SAPI_MODULE_PROPERTIES
};


int sapi_php_startup(sapi_module_struct* sapi_module) {
	return php_module_startup(sapi_module, nullptr);
}

size_t sapi_ub_write(const char* str, size_t str_length) {
	auto context = SG(server_context);
	return ref_ub_write(context, str, str_length);
}

void sapi_flush(void* server_context) {
	auto context = SG(server_context);
	ref_flush(context);
}

int sapi_send_headers(sapi_headers_struct* sapi_headers) {
	char buf[SAPI_MAX_HEADER_LENGTH];
	sapi_header_struct* h;
	zend_llist_position pos;
	bool ignore_status = 0;
	int response_status = SG(sapi_headers).http_response_code;
	auto context = SG(server_context);


	if (SG(sapi_headers).http_response_code != 200) {
		int len;
		bool has_status = 0;
		char* s;

		if (SG(sapi_headers).http_status_line &&
			(s = strchr(SG(sapi_headers).http_status_line, ' ')) != 0 &&
			(s - SG(sapi_headers).http_status_line) >= 5 &&
			strncasecmp(SG(sapi_headers).http_status_line, "HTTP/", 5) == 0
		) {
			len = slprintf(buf, sizeof(buf), "Status:%s", s);
			response_status = atoi((s + 1));
		} else {
			h = (sapi_header_struct*)zend_llist_get_first_ex(&sapi_headers->headers, &pos);
			while (h) {
				if (h->header_len > sizeof("Status:") - 1 &&
					strncasecmp(h->header, "Status:", sizeof("Status:") - 1) == 0
					) {
					has_status = 1;
					break;
				}
				h = (sapi_header_struct*)zend_llist_get_next_ex(&sapi_headers->headers, &pos);
			}
			if (!has_status) {
				http_response_status_code_pair* err = (http_response_status_code_pair*)http_status_map;

				while (err->code != 0) {
					if (err->code == SG(sapi_headers).http_response_code) {
						break;
					}
					err++;
				}
				if (err->str) {
					len = slprintf(buf, sizeof(buf), "Status: %d %s", SG(sapi_headers).http_response_code, err->str);
				} else {
					len = slprintf(buf, sizeof(buf), "Status: %d", SG(sapi_headers).http_response_code);
				}
			}
		}

		if (!has_status) {
			ref_send_header(context, buf, len);
			ignore_status = 1;
		}
	}

	h = (sapi_header_struct*)zend_llist_get_first_ex(&sapi_headers->headers, &pos);
	while (h) {
		/* prevent CRLFCRLF */
		if (h->header_len) {
			if (h->header_len > sizeof("Status:") - 1 &&
				strncasecmp(h->header, "Status:", sizeof("Status:") - 1) == 0
				) {
				if (!ignore_status) {
					ignore_status = 1;
					ref_send_header(context, h->header, h->header_len);
				}
			} else if (response_status == 304 && h->header_len > sizeof("Content-Type:") - 1 &&
				strncasecmp(h->header, "Content-Type:", sizeof("Content-Type:") - 1) == 0
				) {
				h = (sapi_header_struct*)zend_llist_get_next_ex(&sapi_headers->headers, &pos);
				continue;
			} else {
				ref_send_header(context, h->header, h->header_len);
			}
		}
		h = (sapi_header_struct*)zend_llist_get_next_ex(&sapi_headers->headers, &pos);
	}

	return SAPI_HEADER_SENT_SUCCESSFULLY;
}

size_t sapi_read_post(char* str, size_t str_length) {
	auto context = SG(server_context);
	return ref_read_post(context, str, str_length);
}

char* sapi_read_cookies() {
	auto context = SG(server_context);
	return ref_read_cookies(context);
}

void sapi_php_register_variables_retn(zval* track_vars_array, char* key, char* val) {
	size_t new_val_len;

	if (sapi_warp_module.input_filter(PARSE_SERVER, key, &val, strlen(val), &new_val_len)) {
		php_register_variable_safe(key, val, new_val_len, track_vars_array);
	}
}

void sapi_php_register_variables(zval* track_vars_array) {
	char* php_self = (char*)(SG(request_info).request_uri ? SG(request_info).request_uri : "");
	size_t php_self_len = strlen(php_self);

	auto context = SG(server_context);
	auto feedback = [track_vars_array](char* key, char* val) {
		size_t new_val_len;

		if (sapi_warp_module.input_filter(PARSE_SERVER, key, &val, strlen(val), &new_val_len)) {
			php_register_variable_safe(key, val, new_val_len, track_vars_array);
		}
	};

	ref_server_param(context, track_vars_array, sapi_php_register_variables_retn);

	if (sapi_warp_module.input_filter(PARSE_SERVER, "PHP_SELF", &php_self, php_self_len, &php_self_len)) {
		php_register_variable_safe("PHP_SELF", php_self, php_self_len, track_vars_array);
	}
}

void sapi_php_log_message(const char* message, int syslog_type_int) {
	fprintf(stderr, "%s\n", message);
}