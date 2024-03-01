#include "main.h"
#include "sapi.h"
#include <thread>


int On(
	TSendHeader send_header,
	TUbWrite ub_write,
	TFlush flush,
	TSapiReadPpost read_post,
	TReadCookies read_cookies,
	TServerParam server_param
) {
	auto php_cwd = std::getenv("XPHP_CWD");
	if (php_cwd) {
		sapi_warp_module.executable_location = (char*)php_cwd;
	}

	auto php_ini = std::getenv("XPHP_INI");
	if (php_ini) {
		sapi_warp_module.php_ini_path_override = (char*)php_ini;
	}

	sapi_warp_module.input_filter = php_default_input_filter;

	ref_ub_write = ub_write;
	ref_flush = flush;
	ref_send_header = send_header;
	ref_read_post = read_post;
	ref_read_cookies = read_cookies;
	ref_server_param = server_param;


#ifdef ZTS
	auto success = php_tsrm_startup();
	//auto success = php_tsrm_startup_ex(8); < PHP 8.3.0
	if (success == 0) {
		return -1;
	}
#endif

	//zend_signal_init(); // This code does not externally use zend_signal_startup().
	zend_signal_startup();
	sapi_startup(&sapi_warp_module);
	if (FAILURE == sapi_warp_module.startup(&sapi_warp_module)) {
		return -2;
	}

	SG(headers_sent) = 0;
	SG(request_info).no_headers = 0;

	return 0;
}
int Off() {
	php_module_shutdown();
	sapi_shutdown();

#ifdef ZTS
	tsrm_shutdown();
#endif

	return 0;
}
int Execute(void* context, char* method, char* content_type, size_t content_length, int filename_index, char* filepath, char* query_string) {
	int state = 0;
	//auto tid = tsrm_thread_id();

#ifdef ZTS
	//ts_resource_ex(0, tid);
	ts_resource_ex(0, 0);
#endif

	SG(server_context) = context;
	SG(sapi_headers).http_response_code = 200;
	SG(request_info).request_method = (char*)method;
	SG(request_info).query_string = (char*)query_string;
	SG(request_info).request_uri = (char*)(filepath + filename_index);
	SG(request_info).content_type = content_type;
	SG(request_info).content_length = content_length;

	if (SUCCESS == php_request_startup()) {
		zend_first_try{
			zval retval;
			zend_file_handle file_handle;
			zend_stream_init_filename(&file_handle, filepath);
			file_handle.primary_script = 1;

			PG(during_request_startup) = 0;
			state = zend_execute_scripts(ZEND_REQUIRE, &retval, 1, &file_handle);
			if(state == SUCCESS){
				state <<= 4;
				state |= Z_LVAL_P(&retval);
				zval_ptr_dtor(&retval);
			}

			zend_destroy_file_handle(&file_handle);
		} zend_catch {

		} zend_end_try();

		php_request_shutdown(nullptr);
	}

#ifdef ZTS
	ts_free_thread();
#endif

	return state;
}