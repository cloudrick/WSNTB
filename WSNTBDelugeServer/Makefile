
CPP = g++
CC = g++
CPPFLAGS = -g -Wall -I/usr/include/mysql -DBIG_JOINS=1
LDFLAGS = -L/usr/lib/mysql -lmysqlclient -lpthread

all: WSNTBDS.o
	$(CPP) $(CPPFLAGS) $(LDFLAGS) -o WSNTBDS WSNTBDS.o
clean: 
	rm -f WSNTBDS.*
	rm -f WSNTBDS
