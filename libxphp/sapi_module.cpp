#include "sapi_module.h"

zend_module_entry nightmare_module_entry = {
    STANDARD_MODULE_HEADER,
    "nightmare",
    ext_functions,
    nullptr,
    nullptr,
    nullptr,
    nullptr,
    nullptr,
    NO_VERSION_YET,
    STANDARD_MODULE_PROPERTIES
};

/*void variable() {
  switch (Z_TYPE(args[i])) {
  case IS_NULL:
    php_printf("Argument %d is NULL\n", i + 1);
    break;
  case IS_LONG:
    php_printf("Argument %d is long and the value is %ld\n", i + 1, Z_LVAL(args[i]));
    break;
  case IS_DOUBLE:
    php_printf("Argument %d is double and the value is %f\n", i + 1, Z_DVAL(args[i]));
    break;
  case IS_STRING:
    php_printf("Argument %d is string and the value is %s\n", i + 1, Z_STRVAL(args[i]));
    break;
  case IS_BOOL:
    php_printf("Argument %d is bool and the value is %s\n", i + 1, Z_LVAL(args[i]) ? "TRUE" : "FALSE");
    break;
  case IS_ARRAY:
    php_printf("Argument %d is array\n", i + 1);
    break;
  case IS_OBJECT:
    php_printf("Argument %d is object and the class name is %s\n", i + 1, Z_OBJCE(args[i])->name->val);
    break;
  default:
    php_printf("Argument %d is of unknown type\n", i + 1);
  }
}*/

PHP_FUNCTION(test1) {
  ZEND_PARSE_PARAMETERS_NONE();

  php_printf("The extension %s is loaded and working!\r\n", "%EXTNAME%");
  /*zval* args;
  int argc;

  if (zend_parse_parameters(ZEND_NUM_ARGS(), "+", &args, &argc) == SUCCESS) {
    for (auto i = 0; i < argc; i++) {

    }
  }
  switch (ZEND_NUM_ARGS()) {
  case 1:
    break;
  case 2:
    break;
  case 3:
    break;
  }*/
}