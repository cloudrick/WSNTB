
/*
 * WSNTBDS.cpp
 * Wireless Sensor Network Test-Bed Deluge Server Deamon
 *
 * Author:
 * Wei-Sheng Yang (Rick) rick@axp1.csie.ncu.edu.tw
 *
 * Copyright By
 * High Speed Communication and Computing Lab (HSCC)
 * National Central University (NCU)
 * 2008
 */

// C++ Standard 
#include <iostream>
#include <cstring>
#include <string>

// POSIX Standard
#include <sys/ioctl.h>
#include <sys/socket.h>
#include <sys/types.h>
#include <fcntl.h>
#include <unistd.h>
#include <netinet/in.h>
#include <pthread.h>

// MySQL
#include<mysql.h>

using namespace std ;

const char SERVER_ADDR[]  = "127.0.0.1" ;
const int  SERVER_PORT   = 35330 ;
const char mysqlServer[]   = "wsn.tw" ;
const char mysqlUser[]     = "testbed" ;
const char mysqlPassword[] = "q2dfb26G" ;
const char mysqlDatabase[] = "testbed" ;

struct sockaddr_in adr_srvr ;
struct sockaddr_in adr_clnt ;

typedef struct str_thdata
{
    int thread_done ;
    int thread_socket ;
} Thdata;

pthread_mutex_t thread_mutex = PTHREAD_MUTEX_INITIALIZER ;


string IntToString(int n)
{
	string ret = "" ;
	while( n!=0 )
	{
		ret = (char)((n%10)+'0') + ret ;
		n/=10 ;
	}
	return ret=="" ? "0" : ret ;
}

/*
void SendLog( void *p)
{
	Thdata *thdata = (Thdata *) p ;

	int c = thdata->thread_socket ;

	for(;;)
	{
		// begin of critical section
		pthread_mutex_lock(&thread_mutex) ;
		if( thdata->thread_done )
		{
			pthread_mutex_unlock(&thread_mutex) ;
			break ;
		}
		pthread_mutex_unlock(&thread_mutex) ;
		// end of critical section

		FILE *fp = fopen("log.txt","r") ;

		char ch ;
		while( fscanf(fp,"%c",&ch)==1 )
		{
			write( c , &ch , 1 ) ;
		}
		fclose(fp) ;
	}
}
*/

void SendLog(int c)
{
	FILE* fp ;
	char buf[256] ;
	
	fp=fopen("log.txt" , "r") ;
	while( fgets(buf,256,fp) )
	{
		write( c , buf , strlen(buf) ) ;
	}
	fclose(fp) ;
 
	fp = fopen("err.txt" , "r") ;
	while( fgets(buf,252,fp) )
	{
		write( c , buf , strlen(buf) ) ;
	}
	fclose(fp) ;
}


int open_socket_connection(const char *srvr_addr , const int srvr_port)
{
	int s ;
	int z ;
	int len_inet ;

	// create TCP/IP socket for server
	s = socket( PF_INET , SOCK_STREAM , 0 ) ;
	if( s == -1 )
	{
		perror("could not create server socket\n") ;
		return -1 ;
	}

	// create server socket address
	memset( &adr_srvr , 0 , sizeof(adr_srvr) ) ;
	adr_srvr.sin_family = PF_INET ;
	adr_srvr.sin_port   = htons( srvr_port ) ;
	adr_srvr.sin_addr.s_addr = INADDR_ANY ;
	if( adr_srvr.sin_addr.s_addr == INADDR_NONE )
	{
		perror("bad address\n") ;
		return -1 ;
	}

	// bind the server socket
	len_inet = sizeof( adr_srvr ) ;
	z = bind ( s , (struct sockaddr *) &adr_srvr , len_inet ) ;
	if( z == -1)
	{
		perror("could not bind the socket\n") ;
		return -1 ;
	}

	// make the socket listening
	z = listen(s,1) ;
	if( z == -1)
	{
		perror("cound not make the socket listening\n") ;
		return -1 ;
	}

	return s ;
}

int main()
{
	// used for network socket
	int s ;

	char buffer[256] ;

	// create TCP/IP socket for server	
	s = open_socket_connection( SERVER_ADDR , SERVER_PORT ) ;

	if( s == -1 )
	{
		return -1 ;
	}


	for(;;)
	{
		int c ;
		int len_inet = sizeof( adr_clnt ) ;

		// wait for a connection
		c = accept( s , (struct sockaddr *) &adr_clnt , (socklen_t*) &len_inet ) ;

		if( c == -1 )
		{
			close(s) ;
			perror("cound not accept a connection\n") ;
			return -1 ;
		}
		else
		{
			cout << "[Connected]" << endl ;
		}

/*
		// shared variable
		Thdata thdata ; 
		thdata.thread_done = 0 ;
		thdata.thread_socket = c ;
		// thread variables 
		pthread_t thread_sendlog ;

		pthread_create (&thread_sendlog, NULL,(void* (*)(void*))SendLog, (void*)&thdata );
*/
		for(;;)
		{
			if( read( c , buffer , 256 ) > 0) 
			{				
				if( strcmp( buffer , "DISCONNECT\0" ) == 0 )
				{
					cout << "[Disconnected]" << endl ;
					break ;
				}
				else if( strncmp( buffer , "AUTH" , 4 ) == 0)
				{
					char auth[6],username[16], password[64] ;
					sscanf(buffer,"%s%s%s",auth,username,password) ;
					
					MYSQL sql_conn ;
					mysql_init( &sql_conn ) ;
					
					if( !mysql_real_connect( &sql_conn , mysqlServer , mysqlUser , mysqlPassword , 
                                                 mysqlDatabase, 0,NULL,0) )
					{
						cout << "Failed to connect to database" << endl ;
						cout << "[Disconnected]" << endl ;
						mysql_error(&sql_conn) ;
						break ;
					}
					
					string sql ;
					sql = "SELECT `uid`, `pass` FROM `user` WHERE `uid`=\'" + string(username) + "\' AND `pass`=\'" + string(password) + "\'" ;
					mysql_query( &sql_conn , sql.c_str()) ;
					
					MYSQL_RES *result = mysql_store_result( &sql_conn ) ;
					MYSQL_ROW record ;
					
					if( result && (record = mysql_fetch_row(result)) ) 
					{	
						mysql_free_result(result) ;
						
						sql = "SELECT `owner` FROM `exp_info` WHERE (`is_ncu1_working`=1 OR `is_ncu2_working`=1 OR `is_nthu1_working`=1) AND `is_cancel`=0" ;
						
						mysql_query( &sql_conn , sql.c_str() ) ;
						MYSQL_RES *result2 = mysql_store_result( &sql_conn ) ;
						MYSQL_ROW record2 ;
						
						if( result2 && (record2 = mysql_fetch_row(result2)) )
						{
							if( strcmp(record2[0] , username ) == 0)
							{
								write(c,"AUTH OK\0",8);
							}
							else
							{
							 	write(c,"AUTH NO\0",8);
							}
							mysql_free_result(result2) ;
						}
						else
						{
							write(c,"AUTH NO\0",8);
						}
					}
					else
					{
						write(c,"AUTH NO\0",8);
					}
					mysql_close(&sql_conn) ;
					
				}
				else if ( strncmp( buffer , "PING" , 4 ) == 0)
				{
					int id ;
					sscanf(buffer,"PING %d",&id) ;
		
					string tmp = "java net.tinyos.tools.Deluge -p -f " ;
					
					if( id > 0 )
					{
						cout << "[Ping " << id << "]" << endl ;
						tmp += "-id=" + IntToString(id) + " " ;
					}
					else
					{
						cout << "[Ping]" << endl ;
					}
					tmp += ">> log.txt 2>> err.txt" ;
					cout << tmp << endl ;
					system(tmp.c_str()) ;

					SendLog(c) ;
					cout << "[EndPing]" << endl ;
				}
				else if ( strncmp( buffer , "INJECT" , 6 ) == 0)
				{
					int id ;
					sscanf(buffer,"INJECT %d",&id) ;
					cout << "[Inject ImgNum=" << id << "]" << endl ;

					char buffer2[16] ;
					int len = 0 ;
					while( read( c , buffer2 , 16 ) > 0)
					{
						if( strncmp( buffer2 , "FILESIZE" , 8) == 0 )
						{
							sscanf(buffer2,"FILESIZE %d",&len) ;
							break ;
						}
					}
					
					FILE *TOS = fopen("tos.xml","wb") ;
					
					for(int i=0 ; i<len ; ++i)
					{
						char ch ;
						read(c,&ch,1);
						fwrite(&ch,1,1,TOS) ;
					}
					fclose(TOS) ;
					
					
					string tmp = "java net.tinyos.tools.Deluge -i -f " ;
					
					
					tmp += "-in=" + IntToString(id) + " -ti=tos.xml " ;
					
					tmp += "> log.txt 2> err.txt" ;
					cout << tmp << endl ;
					system(tmp.c_str()) ;
					system("rm -f tos.xml");

					SendLog(c) ;
					
					cout << "[EndInject]" << endl ;
				}
				else if( strncmp( buffer , "REBOOT" , 6 ) == 0)
				{
					int id ;
					sscanf(buffer,"REBOOT %d",&id) ;
		
					string tmp = "java net.tinyos.tools.Deluge -r -f " ;
					
					cout << "[Reboot ImgNum=" << id << "]" << endl ;
					tmp += "-in=" + IntToString(id) + " " ;
					
					tmp += "> log.txt 2> err.txt" ;
					cout << tmp << endl ;
					system(tmp.c_str()) ;

					SendLog(c) ;
					cout << "[EndReboot]" << endl ;
				}
				else if( strncmp( buffer , "ERASE" , 2 ) == 0)
				{
					int id ;
					sscanf(buffer,"ERASE %d",&id) ;
		
					string tmp = "java net.tinyos.tools.Deluge -e -f " ;
					
					cout << "[Erase ImgNum=" << id << "]" << endl ;
					tmp += "-in=" + IntToString(id) + " " ;
					
					tmp += "> log.txt 2> err.txt" ;
					cout << tmp << endl ;
					system(tmp.c_str()) ;

					SendLog(c) ;
					cout << "[EndErase]" << endl ;
				}
				else if( strncmp( buffer , "RESET" , 5 ) == 0)
				{
					int id ;
					sscanf(buffer,"RESET %d",&id) ;
		
					string tmp = "java net.tinyos.tools.Deluge -x -f " ;
					
					cout << "[Reset ImgNum=" << id << "]" << endl ;
					tmp += "-in=" + IntToString(id) + " " ;

					tmp += "> log.txt 2> err.txt" ;
					cout << tmp << endl ;
					system(tmp.c_str()) ;

					SendLog(c) ;
					cout << "[EndReset]" << endl ;
				}

				
				//cout << buffer << endl ;
			}
			system("rm -f log.txt");
			system("rm -f err.txt") ;
		}

		// ssize_t read(int, void*, size_t)

		// ssize_t write(int, const void*, size_t)	

		/*
		// wait for thread done
		pthread_mutex_lock(&thread_mutex) ;
		thdata.thread_done = 1 ;
		pthread_mutex_unlock(&thread_mutex) ;
		pthread_join(thread_sendlog, NULL);
		*/

		close(c) ;
	}

	close(s) ;


	return 0 ;
}
