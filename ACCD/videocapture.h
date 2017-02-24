

#define WEBCAM      	"/dev/video0" 
#define QUALITY_DEFAULT	80 

#define FMT_UNKNOWN	0
#define FMT_PPM		1
#define FMT_JPEG	2
#define FMT_PNG		3

#define IN_TV		0
#define IN_COMP1	1
#define IN_COMP2	2
#define IN_SVIDEO	3

#define NORM_PAL	0
#define NORM_NTSC	1
#define NORM_SECAM	2

unsigned char* 	get_image (int,int, int,int,unsigned long,int) ;
void		free_image(char *);
void 		put_image_jpeg (FILE*,char*,int,int,int,int) ;

