
CC = arm-linux-gcc
AR = arm-linux-ar r
ranlib = arm-linux-ranlib

CFLAGS = -g -O2 -I/usr/local/arm/include 
LDFLAGS =  -L/usr/local/arm/lib 
LIBS = -ljpeg -lpthread 

default: accd

accd: accd.o videocapture.o v4l.o
	$(CC) $(LDFLAGS) -o $@ accd.o videocapture.o v4l.o $(LIBS)
acc:  acc.o
	$(CC) $(LDFLAGS) -o $@ acc.o $(LIBS)
test: test.o
	$(CC) $(LDFLAGS) -o $@ test.o
clean:
	rm -f *.o
	rm -f acc
	rm -f accd
	rm -f test

