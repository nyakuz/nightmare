#pragma once
#ifndef LIB_MAIN
#define LIB_MAIN

#include "ref.h"
#include <cstddef>
#include <functional>

#include <main/php.h>
#include <main/http_status_codes.h>
#include <main/SAPI.h>
#include <main/php_main.h>


#if defined(ZTS)
ZEND_TSRMLS_CACHE_EXTERN();
#endif

extern "C" {
	THREAD_T tsrm_thread_id(void);
	int On(
		TSendHeader,
		TUbWrite,
		TFlush,
		TSapiReadPpost,
		TReadCookies,
		TServerParam
	);
	int Off();
	int Execute(void*, char*, char*, size_t, int, char*, char*);
}
#endif