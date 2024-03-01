#pragma once
#include <cstddef>
#include <functional>

#include "http_status_codes.h"
#include <main/php.h>
#include <main/SAPI.h>
#include <main/php_main.h>

typedef void   (*TServerParamRetn)(zval*, char*, char*);

typedef void   (*TSendHeader)(void*, char*, size_t);
typedef size_t (*TUbWrite)(void*, const char*, size_t);
typedef int    (*TFlush)(void*);
typedef size_t (*TSapiReadPpost)(void*, char*, size_t);
typedef char*  (*TReadCookies)(void*);
typedef void   (*TServerParam)(void*, zval*, TServerParamRetn);

extern TSendHeader ref_send_header;
extern TUbWrite ref_ub_write;
extern TFlush ref_flush;
extern TSapiReadPpost ref_read_post;
extern TReadCookies ref_read_cookies;
extern TServerParam ref_server_param;