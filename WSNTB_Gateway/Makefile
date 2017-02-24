# A simple makefile to cause make to go look in the top directory. A simple
# convenience.

CPP = g++

CPPFLAGS = -g -Wall -I/usr/include/mysql -DBIG_JOINS=1
LDFLAGS = -L/usr/lib/mysql -lmysqlclient

all: TBGateway.o 
	$(CPP) $(CPPFLAGS) $(LDFLAGS) -o TBGateway TBGateway.o
clean: 
	rm -f TBGateway.*
	rm -f TBGateway
