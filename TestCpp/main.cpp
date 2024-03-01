#include <main.h>
#include <cstdio>
#include <cstddef>
#include <vector>
#include <thread>
#include <iostream>
#include <chrono>
#include <algorithm>


void   send_header(void* thread_id, char* head, size_t str_length){
	//printf("%s\n",head);
}
size_t ub_write(void* thread_id, const char* str, size_t str_length){
	printf("%s", str);
	//printf("%ld %s\n",str_length,str);
	return str_length;
}
int   flush(void* thread_id){
	return 0;
}
size_t read_post(void* thread_id, char* buf, size_t content_bytes){
	return 0;
}
char*  read_cookies(void* thread_id){
	return (char*)"e=";
	//return (char*)"a=A00; b=B00; c=C00";
}

void   server_param(void* thread_id, zval* track_vars_array, TServerParamRetn retn){
	retn(track_vars_array, (char*)"DOCUMENT_ROOT", (char*)"/Nightmare/libxphp/testcpp/");
	retn(track_vars_array, (char*)"HTTP_TEST", (char*)"123");
}


const auto script_filename = "main.php";

int main(int argc, char* argv[]) {
	auto ret = On(
		send_header,
		ub_write,
		flush,
		read_post,
		read_cookies,
		server_param
	);


	auto tid = (void*)0;
	auto code = Execute(
		tid,
		(char*)"GET",
		(char*)"",
		0,
		7,
		(char*)script_filename,
		(char*)""
	);
/*
	char line[32];
	for(;;){
		for(auto i=0;i<1000;i++){
			auto code = Execute(
				thread_id_callback,
				(char*)"GET",
				(char*)"",
				0,
				53,
				(char*)script_filename,
				(char*)""
			);
		}

		const auto p2 = std::chrono::system_clock::now();
		std::cout << "2: " << std::chrono::duration_cast<std::chrono::seconds>(p2.time_since_epoch()).count() << '\n';
	}
*/
	//Off();
}