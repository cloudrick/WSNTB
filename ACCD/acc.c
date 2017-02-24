
/*
 * acc.c
 * Simple Autonomic Car Controller
 *
 * Author:
 * Wei-Sheng Yang (Rick) rick@axp1.csie.ncu.edu.tw
 * Cheng-Yu Chung (Chris)  chris@axp1.csie.ncu.edu.tw
 *
 * Copyright by 
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

#include <termios.h>
#include <fcntl.h>
#include <unistd.h>
#include <assert.h>
#include <netinet/in.h>
#include <pthread.h>

#define SERIAL_PORT "/dev/tts/0" 
#define UART_PORT   "/dev/tts/2"
#define SERVER_ADDR "127.0.0.1"
#define SERVER_PORT "8503"


typedef struct str_thdata
{
    int thread_done ;
    int rs232Ctrl ;
} Thdata;


void 	print_signal_status(int) ;
int 	getch() ;

// used for threads
pthread_mutex_t mutex_rs232_receiver = PTHREAD_MUTEX_INITIALIZER ;


int getch(void)
{
      	int c=0;

      	struct termios org_opts, new_opts;
      	int res=0;
	
	//-----  store old settings -----------
      	res=tcgetattr(STDIN_FILENO, &org_opts);
      	assert(res==0);
        
	//---- set new terminal parms --------
	memcpy(&new_opts, &org_opts, sizeof(new_opts));
        new_opts.c_lflag &= ~(ICANON | ECHO | ECHOE | ECHOK | ECHONL | ECHOPRT | ECHOKE | ICRNL);
      	tcsetattr(STDIN_FILENO, TCSANOW, &new_opts);
      	c=getchar();

        //------  restore old settings ---------
      	res=tcsetattr(STDIN_FILENO, TCSANOW, &org_opts);
      	assert(res==0);

      	return(c);
}

// print rs232 signal status
void print_signal_status(int status) 
{
	printf("CAR : ") ;
	if( status & TIOCM_CAR )
		printf("1\n") ;
	else
		printf("0\n") ;

	printf("SR  : ") ;
	if( status & TIOCM_SR )
		printf("1\n") ;
	else
		printf("0\n") ;

	printf("ST  : ") ;
	if( status & TIOCM_ST )
		printf("1\n") ;
	else
		printf("0\n") ;

	printf("DTR : ") ;
	if( status & TIOCM_DTR )
		printf("1\n") ;
	else
		printf("0\n") ;

	printf("DSR : ") ;
	if( status & TIOCM_DSR )
		printf("1\n") ;
	else
		printf("0\n") ;

	printf("RTS : ") ;
	if( status & TIOCM_RTS )
		printf("1\n") ;
	else
		printf("0\n") ;

	printf("CTS : ") ;
	if( status & TIOCM_CTS )
		printf("1\n") ;
	else
		printf("0\n") ;

	printf("RNG : ") ;
	if( status & TIOCM_RNG )
		printf("1\n") ;
	else
		printf("0\n") ;
}

void rs232Receiver(void *ptr)
{
	Thdata *thdata = (Thdata*)(ptr) ;
	char buf[8] ;
	int len ;
	memset( buf , 0 , 8 ) ;

	for(;;)
	{
		pthread_mutex_lock(&mutex_rs232_receiver) ;
		if( thdata->thread_done )
		{
			pthread_mutex_unlock(&mutex_rs232_receiver) ;
			break ;
		}
		pthread_mutex_unlock(&mutex_rs232_receiver) ;

		len = read( thdata->rs232Ctrl , buf , 7 ) ;

		if( len > 0 )
		{
			buf[len] = '\0' ;
			printf("%d byte received from rs232 : %s\n",len,buf) ;
		}
		usleep(10000) ;
	}

}

void readUART(int ctrl) 
{
	unsigned char buf[512] ;
	unsigned char c ;
	int len_buf ;
	int len_c ;
	int i ;

	memset( buf , 0 , 512 ) ;
	len_buf = 0 ;

	for(;;)
	{
		len_c = read( ctrl , &c , 1 ) ;
		
		if( len_c > 0)
		{						
			if( c == 0x7e )
			{
				if( len_buf > 1 )
				{
					
					printf( "received %d byte from UART \n",len_buf) ;
					for( i=0 ; i< len_buf ; ++i)
					{
						printf("%02x",buf[i]) ;
					}
					printf("7e\n") ;
				}

				len_buf = 0 ;
				buf[len_buf++] = 0x7e ;
				
			}
			else
			{
				if( len_buf >=512 )
				{
					printf("the length is larger than 512\n") ;
					break ;
				}

				buf[ len_buf++ ] = c ;

			}

		}
	}

}

int main(int argc, const char* argv[])
{
	// used for serial port control
	int status ;		// serial port status
	int rs232Ctrl ;		// control of serial port
	int uartCtrl ;
	struct termios rs232Options ;
	struct termios uartOptions ;

	// thread variables 
	pthread_t rs232_receiver_thread ;
	Thdata thdata ; // shared variable


	// used for debug mode
	int k1,k2,k3 ;
	
	/*
	// open the serial port
	rs232Ctrl = open( SERIAL_PORT , O_RDWR | O_NOCTTY | O_NDELAY) ;

	if( rs232Ctrl < 0 )
	{
		perror("could not open /dev/tts/0\n") ;
		return -1 ;
	}	

	// change the current options for the port
	tcgetattr( rs232Ctrl , &rs232Options) ;
	cfsetispeed(&rs232Options, B9600);
	cfsetospeed(&rs232Options, B9600);
	rs232Options.c_cflag |= ( CLOCAL | CREAD | CS8 );
	rs232Options.c_cflag &= ~PARENB ;
	rs232Options.c_cflag &= ~CSTOPB ;
	rs232Options.c_cflag &= ~CRTSCTS ;
	tcflush( rs232Ctrl , TCIFLUSH);
	tcsetattr( rs232Ctrl , TCSANOW, &rs232Options);
*/
	// open the UART serial port
	uartCtrl = open( UART_PORT , O_RDWR | O_NOCTTY ) ;

	if( uartCtrl < 0 )
	{
		perror("could not open /dev/tts/2\n") ;
		return -1 ;
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

	readUART( uartCtrl ) ;

/*

	// create thread to receive data from rs232
	thdata.thread_done = 0 ;
	thdata.rs232Ctrl = rs232Ctrl ;
	pthread_create ( &rs232_receiver_thread, NULL, (void *) &rs232Receiver, (void*) &thdata );

	// wait for keyin
	for(;;)
	{
		k1 = getch() ;
	
		switch(k1)
		{
		case 27:
			k2 = getch() ;
			
			switch( k2 )
			{
				case 91 :
					k3 = getch() ;
					switch(k3)
					{
					case 65 :
						printf("UP\n") ;
						write( rs232Ctrl , "A" , 1 ) ;
						break ;
					case 66 :
						printf("DOWN\n") ;
						//write( rs232Ctrl , "" , 1 ) ;
						break ;
					case 67 :
						printf("RIGHT\n") ;
						write( rs232Ctrl , "B" , 1 ) ;
						break ;
					case 68 :
						printf("LEFT\n") ;
						write( rs232Ctrl , "C" , 1 ) ;
						break ;
					default :
						printf("Unknow key in k3: %d\n",k3) ;
					}

					break ;
				case 27 :
					printf("ESC\n") ;

					pthread_mutex_lock(&mutex_rs232_receiver) ;
					thdata.thread_done = 1 ;
					pthread_mutex_unlock(&mutex_rs232_receiver) ;

					pthread_join( rs232_receiver_thread , NULL) ;

					close(rs232Ctrl) ;
					return 0 ;

				default :
					printf("Unknow key in k2: %d\n",k2) ;
			}
			break ;
		case 32:
			printf("STOP\n") ;
			write( rs232Ctrl , "D" , 1 ) ;
			break ;
		default :
			printf("Unknow key in k1: %d\n",k1) ;
			break ;
		}
			
	}

	//print_signal_status(status) ;

	close(rs232Ctrl) ; */
	close(uartCtrl) ;
			
	return 0 ;
}
