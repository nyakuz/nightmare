#pragma once
#ifndef LIB_NIGHTMARE
#define LIB_NIGHTMARE

#include <main/php.h>
#include <zend_API.h>
#include <zend_modules.h>

extern zend_module_entry nightmare_module_entry;

ZEND_BEGIN_ARG_WITH_RETURN_TYPE_INFO_EX(arginfo_test1, 0, 0, IS_VOID, 0)
ZEND_END_ARG_INFO()

ZEND_FUNCTION(test1);

static const zend_function_entry ext_functions[] = {
    PHP_FE(test1, arginfo_test1)
    PHP_FE_END
};

#endif