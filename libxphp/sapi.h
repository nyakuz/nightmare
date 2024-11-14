#pragma once
#ifndef LIB_XPHP
#define LIB_XPHP

#include "ref.h"
#include <cstddef>
#include <functional>

#include <main/php.h>
#include <main/http_status_codes.h>
#include <main/SAPI.h>
#include <main/php_main.h>
#include <main/php_variables.h>


extern sapi_module_struct sapi_warp_module;

int    sapi_php_startup(sapi_module_struct*);
size_t sapi_ub_write(const char*, size_t);
void   sapi_flush(void*);
int    sapi_send_headers(sapi_headers_struct*);
size_t sapi_read_post(char*, size_t);
char*  sapi_read_cookies();
void   sapi_php_register_variables(zval*);
void   sapi_php_log_message(const char*, int);

#endif