#include <main.h>
#include <main/php.h>
#include <main/SAPI.h>
#include <main/php_main.h>
#include <cstdio>
#include <cstddef>
#include <vector>
#include <thread>
#include <iostream>
#include <chrono>
#include <algorithm>
#include <sys/time.h>
#include <sys/resource.h>


#define PAGE_SIZE 4096
#define STK_SIZE (4 * PAGE_SIZE)

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
	printf("Hello World!\n");
	printf("PHP on\n");
	On(
		send_header,
		ub_write,
		flush,
		read_post,
		read_cookies,
		server_param
	);
	Off();
	On(
		send_header,
		ub_write,
		flush,
		read_post,
		read_cookies,
		server_param
	);


	Execute(
		(void*)0,
		(char*)"GET",
		(char*)"",
		0,
		7,
		(char*)script_filename,
		(char*)""
	);

/*
	struct rlimit rlim;
	getrlimit(RLIMIT_NPROC, &rlim);
	printf("soft limit: %d\n", (int)rlim.rlim_cur);
	printf("hard limit: %d\n\n", (int)rlim.rlim_max);

	rlim.rlim_cur *= 2;
	rlim.rlim_max *= 2;
	setrlimit(RLIMIT_NPROC, &rlim);

	const auto SIZE = 10000;
	auto count = 0;
	pthread_t threads[SIZE];
	for(auto i=SIZE;i>0;i--){
		auto ret = pthread_create(&threads[i], NULL, [](void* arg) -> void* {
			Execute(
				(void*)0,
				(char*)"GET",
				(char*)"",
				0,
				7,
				(char*)script_filename,
				(char*)""
			);

			return NULL;
		}, NULL);

		if (ret == 0) {
			count++;
		}
	}

	void* stack;
	pthread_attr_t attr;
	posix_memalign(&stack, PAGE_SIZE, STK_SIZE);
	pthread_attr_init(&attr);
	pthread_attr_setstack(&attr, &stack, STK_SIZE);

	printf("total thread: %d\n", count);
	printf("Press Any Key to Continue\n");
	getchar();

	for(auto i= count-1;i>=0;i--){
		pthread_join(threads[i], NULL);
	}
*/
/*
	char line[32];
	for(;;){
		for(auto i=0;i<1000;i++){
			auto code = Execute(
				(void*)0,
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
/*
	printf("PHP off\n");
	Off();
*/
	printf("Done\n");
}