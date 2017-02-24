
/*
 * accd.c
 * Autonomic Car Controller Server Daemon
 *
 * Author:
 * Wei-Sheng Yang (Rick) rick@axp1.csie.ncu.edu.tw
 * Cheng-Yu Chung (Chris)  chris@axp1.csie.ncu.edu.tw
 *
 * Copyright By
 * High Speed Communication and Computing Lab (HSCC)
 * National Central University (NCU)
 *
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <sys/ioctl.h>
#include <sys/socket.h>
#include <sys/types.h>
#include <linux/videodev.h>

#include <termios.h>
#include <fcntl.h>
#include <unistd.h>
#include <assert.h>
#include <netinet/in.h>
#include <pthread.h> 

#include "videocapture.h"
#include "v4l.h"


#define SERIAL_PORT 	"/dev/tts/0" 
#define UART_PORT	"/dev/tts/2"
#define SERVER_ADDR 	"127.0.0.1"
#define SERVER_PORT 	8503

typedef struct str_thdata
{
    int thread_done ;
    int thread_socket ;
} Thdata;

pthread_mutex_t thread_mutex = PTHREAD_MUTEX_INITIALIZER ;

struct sockaddr_in adr_srvr ;
struct sockaddr_in adr_clnt ;

// prototype for thread routine

void thread_main_control(void *ptr)
{
	int rs232Ctrl ;
	int c ;
	struct termios rs232Options ;
	char buffer[16] ;
	Thdata *thdata = (Thdata *)ptr ;

	// open the serial port
	rs232Ctrl = open( SERIAL_PORT , O_RDWR | O_NOCTTY | O_NDELAY) ;

	if( rs232Ctrl < 0 )
	{
		pthread_mutex_lock(&thread_mutex) ;
		thdata->thread_done = 1 ;
		pthread_mutex_unlock(&thread_mutex) ;

		perror("could not open serial port\n") ;
		return ;
	}

	// change the options for the port
	tcgetattr( rs232Ctrl , &rs232Options) ;
	cfsetispeed(&rs232Options, B9600);
	cfsetospeed(&rs232Options, B9600);
	rs232Options.c_cflag |= ( CLOCAL | CREAD );
	rs232Options.c_cflag |= CS8 ;
	rs232Options.c_cflag &= ~PARENB ;
	rs232Options.c_cflag &= ~CSTOPB ;
	rs232Options.c_cflag &= ~CRTSCTS;
	tcsetattr( rs232Ctrl , TCSANOW, &rs232Options);

	c = thdata->thread_socket ;

	for(;;)
	{

		pthread_mutex_lock(&thread_mutex) ;
		if( thdata->thread_done )
		{
			pthread_mutex_unlock(&thread_mutex) ;
			break ;
		}
		pthread_mutex_unlock(&thread_mutex) ;

		read ( c , buffer , 15 ) ;

		if( strcmp(buffer , "ACC_DISCONNECT") == 0 )
		{
			pthread_mutex_lock(&thread_mutex) ;
			thdata->thread_done = 1 ;
			pthread_mutex_unlock(&thread_mutex) ;
			break ;
		}
		else if( strcmp( buffer , "ACC_FORWARD") == 0 )
		{
			printf("UP\n") ;
			write( rs232Ctrl , "A\0" , 2 ) ;
		}
		else if( strcmp( buffer , "ACC_RIGHT") == 0 )
		{
			printf("RIGHT\n") ;
			write( rs232Ctrl , "B\0" , 2 ) ;
		}
		else if( strcmp( buffer , "ACC_LEFT") == 0 )
		{
			printf("LEFT\n") ;
			write( rs232Ctrl , "C\0" , 2 ) ;
		}
		else if( strcmp( buffer , "ACC_STOP") == 0 )
		{
			printf("STOP\n") ;
			write( rs232Ctrl , "D\0" , 2 ) ;
		}

		printf("Received from client : %s\n", buffer ) ;
	}

	close(rs232Ctrl) ;
	printf("thread (main_control done)\n") ;
	pthread_exit(0);

}

void thread_send_image(void *ptr)
{
	int c ;

	int width=320, height=240 ;
	int dev = open ( WEBCAM , O_RDWR);
	int input = INPUT_DEFAULT;
	int freq = 0 ;
	int palette = VIDEO_PALETTE_YUV420P;

	int quality = QUALITY_DEFAULT ;
	int color = 1 ;
	int i,j,k;

	char *image_new ;
	char *image_old ;
	int r ;
	int rand_cnt ;
	int err ;

	char data[9] ;

//	char *image = get_image (dev, width, height, input,freq, palette);
//	put_image_jpeg ( (FILE*)jpeg_output ,image, width, height, quality, color);
//	free(image) ;


	Thdata *thdata = (Thdata *)ptr ;
	c = thdata->thread_socket ;

	image_old = get_image (dev, width, height, input,freq, palette);

	data[0]= 0x7a ;
	data[8]= 0x7a ;

	for(;;)
	{
		pthread_mutex_lock(&thread_mutex) ;
		if( thdata->thread_done )
		{
			pthread_mutex_unlock(&thread_mutex) ;
			break ;
		}
		pthread_mutex_unlock(&thread_mutex) ;

		image_new = get_image (dev, width, height, input,freq, palette);

		for( i=0 ; i<240 ; ++i)
		{
			for( j=0 ; j<960 ; j+=3)
			{
				data[3] = i/256 ;
				data[4] = i%256 ;
				data[1] = (j/3)/256 ;
				data[2] = (j/3)%256 ;
				data[5] = image_new[i*960+j] ;
				data[6] = image_new[i*960+j+1] ;
				data[7] = image_new[i*960+j+2] ;
		
				printf("%02X %02X %02X %02X %02X %02X %02X %02X\n",data[0],data[1],data[2],data[3],data[4],data[5],data[6],data[7]);

				write(c,data,9) ;
			}
		}
		
	
/*
		for(i=0 ; i<=95 ; ++i)
		{
			rand_cnt = 50 ;

			while( rand_cnt-- )
			{
				r = rand()%1057 ;
				err = image_new[(i+i+i+1)*1056+r] - image_old[(i+i+i+1)*1056+r] ;

				if( err >= 15 || err<=-15)
				{
					r/=3 ;
					for(j=i+i+i ; j<=i+i+i+2 ; ++j)
					{
						for(k= (r-3<0?0:r-3) ; k<= (r+3>352?352:r+3) ; ++k)	
						{
//							printf("k=%d ",k) ;
							data[1] = j/256 ;
							data[2] = j%256 ;
							data[3] = k/256 ;
							data[4] = k%256 ;
							data[5] = image_new[j*1056+k*3] ;
							data[6] = image_new[j*1056+k*3+1] ;
							data[7] = image_new[j*1056+k*3+2] ;

//							printf("%02X %02X %02X %02X %02X %02X %02X %02X\n",data[0],data[1],data[2],data[3],data[4],data[5],data[6],data[7]);
							write(c,data,8) ;
						}
					}
				}
			}	
		}
*/
		
		free(image_old) ;
		image_old = image_new ;

	}

	free(image_old) ;
	printf("thread (send_image done)\n") ;
	pthread_exit(0);
}

void thread_send_motedata( void *ptr )
{
	int uartCtrl ;
	int c ;
	struct termios uartOptions ;

	unsigned char buf[256] ;
	unsigned char ch ;
	int len_buf ;
	int i ;


	Thdata *thdata = (Thdata *)ptr ;

	// open UART port
	uartCtrl = open( UART_PORT , O_RDWR | O_NOCTTY ) ;

	if( uartCtrl < 0 )
	{
		pthread_mutex_lock(&thread_mutex) ;
		thdata->thread_done = 1 ;
		pthread_mutex_unlock(&thread_mutex) ;

		perror("could not open /dev/tts/2\n") ;
		return ;
	}	

	// change the current options for the port
	tcgetattr( uartCtrl , &uartOptions) ;
	cfsetispeed(&uartOptions, B57600);
	cfsetospeed(&uartOptions, B57600);
	uartOptions.c_cc[VMIN] = 1 ;
	uartOptions.c_cflag |= ( CLOCAL | CREAD | CS8 );
	uartOptions.c_iflag = IGNBRK | IGNPAR ;
	tcflush( uartCtrl , TCIFLUSH);
	tcsetattr( uartCtrl , TCSANOW, &uartOptions);

	memset( buf ,'\0' , 256 ) ;
	len_buf = 0 ;

	c = thdata->thread_socket ;

	for(;;)
	{
		pthread_mutex_lock(&thread_mutex) ;
		if( thdata->thread_done )
		{
			pthread_mutex_unlock(&thread_mutex) ;
			break ;
		}
		pthread_mutex_unlock(&thread_mutex) ;

		len_buf=0 ;
		buf[len_buf] = 0x7E ;

		for(;;)
		{
			if( read( uartCtrl , &ch , 1 ) == 1 )
			{
				buf[++len_buf] = ch ;
				if( ch == 0x7E )
				{
					break ;	
				}
			}
		}

		if( len_buf < 2)
			continue ;
		
		write(c,buf,len_buf) ;

		printf("received %d byte from UART :\n",len_buf) ;
		for( i=0 ; i< len_buf ; ++i)
		{
			printf("%02x",buf[i]) ;
		}
		printf("\n") ;
		
	}
	printf("thread (send_motedata done)\n") ;
	pthread_exit(0);
	
}


int open_socket_connection(char *srvr_addr , int srvr_port)
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
	z = listen(s,2) ;
	if( z == -1)
	{
		perror("cound not make the socket listening\n") ;
		return -1 ;
	}

	return s ;
}

int main(int argc, const char* argv[])
{

	// used for network socket
	char *srvr_addr = SERVER_ADDR ;
	char *srvr_port = SERVER_PORT ;
	int s ;
	int c ;
	int len_inet ;

	char buffer[256] ;

	// thread variables 
	pthread_t thread_mainctrl ;
	pthread_t thread_sendimage ;
	pthread_t thread_sendmotedata ;
	// shared variable
	Thdata thdata ; 	

	// create TCP/IP socket for server	
	s = open_socket_connection( SERVER_ADDR , SERVER_PORT ) ;
	if( s == -1 )
	{
		return -1 ;
	}


	for(;;)
	{
		// wait for a connection
		len_inet = sizeof( adr_clnt ) ;

		c = accept( s , (struct sockaddr *) &adr_clnt , &len_inet ) ;

		if( c == -1 )
		{
			close(s) ;
			perror("cound not accept a connection\n") ;
			return -1 ;
		}

		printf("connection accepted\n") ;
		
		// begin threads
		
		printf("begin threads...\n") ;
		thdata.thread_done = 0 ;
		thdata.thread_socket = c ;
		pthread_create (&thread_mainctrl, NULL, (void *)&thread_main_control, (void*) &thdata );
		//pthread_create (&thread_sendimage, NULL, (void *)&thread_send_image, (void*) &thdata );
		pthread_create (&thread_sendmotedata, NULL, (void *)&thread_send_motedata, (void*) &thdata );
		
		printf("waiting for threads completed\n") ;
		pthread_join(thread_mainctrl, NULL);
		//pthread_join(thread_sendimage, NULL);
		pthread_join(thread_sendmotedata, NULL);
		close(c) ;
		printf("end threads\n") ;
	}

	close(s) ;

	return 0 ;

}
