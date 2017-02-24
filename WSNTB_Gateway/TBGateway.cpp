
/*
 * Program Name: WSNTBG (WSN Testbed Gateway)
 *
 * Author:
 * Wei-Sheng Yang (Rick) rick@axp1.csie.ncu.edu.tw
 *
 * Copyright By
 * High Speed Communication and Computing Lab (HSCC)
 * National Central University (NCU)
 * 2008
 *
 */

// C++ Standard 
#include <iostream>
using namespace std ;

// POSIX Standard
#include <sys/ioctl.h>
#include <termios.h>
#include <fcntl.h>
#include <unistd.h>

// Mysql
#include <mysql.h>

// const
const char UART_PORT[] = "/dev/ttyUSB0" ;

int OpenOctopus2(const char *port)
{
	int ctrl = open( port , O_RDWR ) ;

        if( ctrl < 0 )
	{
		return -1 ;
	}

        // change the current options for the port
        struct termios uartOptions ;
        int status ;
        ioctl( ctrl , TIOCMGET , &status ) ;        
        status &= ~TIOCM_DTR ;
        ioctl( ctrl , TIOCMSET , &status ) ;
        tcgetattr( ctrl  , &uartOptions) ;
        cfsetispeed(&uartOptions , B57600);
        cfsetospeed(&uartOptions , B57600);
        tcsetattr( ctrl , TCSANOW, &uartOptions );
        
        return ctrl ;
}

int main()
{
	int uartCtrl ;

	// open Octopus2 UART port
	if ( (uartCtrl = OpenOctopus2(UART_PORT) ) < 0)
	{
                cerr << "UART Port open error!" << endl ;
                return -1 ;
	}

        // packet buffer and length
	unsigned char buf[256] ;
	int len_buf = 0 ;

        // super loop
	for(;;)
	{
		len_buf=0 ;
		buf[len_buf] = 0x7E ;   // first leading byte
	
                // read the full packet
		for(;;)
		{
			unsigned char ch ;
			if( read( uartCtrl , &ch , 1 ) == 1 )
			{
				buf[++len_buf] = ch ;
				if( ch == 0x7E )
				{
					break ;	
				}
			}
		}

                // ignore the packet which is too short
		if( len_buf < 3)
                {
			continue ;
                }
                
                // print the full packet
		printf("Received %d byte from UART :\n",len_buf) ;
		for( int i=0 ; i< len_buf ; ++i)
		{
			printf("%02x",buf[i]) ;
		}
		printf("\n") ;
		
	}

	return 0 ;
	
}

