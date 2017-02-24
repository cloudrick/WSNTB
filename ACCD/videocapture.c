
#include <stdio.h>
#include <sys/ioctl.h>
#include <sys/mman.h>

#include <linux/types.h>
#include <linux/videodev.h>
#include <jpeglib.h>

#include "videocapture.h"
#include "v4l.h"

//read rgb image from v4l device and return new allocated buffer

unsigned char* get_image (int dev, int width, int height, int input,unsigned long freq, int palette)
{
	struct video_mbuf vid_buf;
	struct video_mmap vid_mmap;
	char *map;
	unsigned char *buff;
	int size, len, bpp;
	register int i;

	if (input == IN_TV) 
	{
		if (freq > 0) 
		{
			if (ioctl (dev, VIDIOCSFREQ, &freq) == -1)
				 perror("ioctl (VIDIOCSREQ) error\n");
		}
	}

	if (palette != VIDEO_PALETTE_GREY)
	{
		/* RGB or YUV */
		size = width * height * 3;
		bpp = 3;
	} 
	else 
	{
		size = width * height * 1;
		bpp = 1;
	}

	vid_mmap.format = palette;

	if (ioctl (dev, VIDIOCGMBUF, &vid_buf) == -1) 
	{
		// do a normal read()
		printf("do a normal read\n") ;
		struct video_window vid_win;

		if (ioctl (dev, VIDIOCGWIN, &vid_win) != -1)
		{
			vid_win.width  = width;
			vid_win.height = height;
			if (ioctl (dev, VIDIOCSWIN, &vid_win) == -1) 
			{
				perror ("ioctl(VIDIOCSWIN) error\n");
				return (NULL);
			}
		}
		map = (char*)malloc (size);
		if (!map)
			return (NULL);
		
		len = read (dev, map, size);
		if (len <= 0) 
		{
			free (map);
			return NULL;
		}

		if (palette == VIDEO_PALETTE_YUV420P) 
		{
			char *convmap;
            		convmap = (char*) malloc ( width * height * bpp );
            		v4l_yuv420p2rgb (convmap, map, width, height, bpp * 8);
            		memcpy (map, convmap, (size_t) width * height * bpp);
            		free (convmap);
        	}
		else if (palette == VIDEO_PALETTE_YUV422P) 
		{
			char *convmap;
		        convmap = (char*)malloc ( width * height * bpp );
            		v4l_yuv422p2rgb (convmap, map, width, height, bpp * 8);
            		memcpy (map, convmap, (size_t) width * height * bpp);
            		free (convmap);
		}
		return (map);
	}

	map = mmap (0, vid_buf.size, PROT_READ|PROT_WRITE,MAP_SHARED,dev,0);
	if ((unsigned char *)-1 == (unsigned char *)map) 
	{
		perror ("mmap() error\n");
		return (NULL);
	}

	vid_mmap.frame = 0;
	vid_mmap.width = width;
	vid_mmap.height =height;

	if (ioctl (dev, VIDIOCMCAPTURE, &vid_mmap) == -1) 
	{
		perror ("ioctl(VIDIOCMCAPTURE) error\n");
		munmap (map, vid_buf.size);
		return (NULL);
	}

	if (ioctl (dev, VIDIOCSYNC, &vid_mmap.frame) == -1) 
	{
		perror("ioctl(VIDIOCSYNC) error\n");
		munmap (map, vid_buf.size);
		return (NULL);
	}

	buff = (unsigned char *) malloc (size);
	if (buff) 
	{
		if (palette == VIDEO_PALETTE_YUV420P)
		{
			v4l_yuv420p2rgb (buff, map, width, height, 24);
        	} 
		else if (palette == VIDEO_PALETTE_YUV422P) 
		{
            		v4l_yuv422p2rgb (buff, map, width, height, 24);
		} 
		else 
		{
			for (i = 0; i < size; i++)
				buff[i] = map[i];
		}
	} 
	else 
	{
		perror ("malloc()");
	}
	munmap (map, vid_buf.size);
	return (buff);
}

void free_image(char *image)
{
	if( image )
	{
		free(image) ;
		image = NULL ;
	}
}

void put_image_jpeg (FILE *output,char *image, int width, int height, int quality, int color)
{
	register int x, y, line_width;
	JSAMPROW row_ptr[1];
	struct jpeg_compress_struct cjpeg;
	struct jpeg_error_mgr jerr;
	char *line = NULL;

	if (color)
	{
		line_width = width * 3;
		line = (char*) malloc (line_width);
		if (!line)
			return;
	} 
	else 
	{
		line_width = width;
	}

	cjpeg.err = jpeg_std_error(&jerr);
	jpeg_create_compress (&cjpeg);
	cjpeg.image_width = width;
	cjpeg.image_height= height;

	if (color)
	{
		cjpeg.input_components = 3;
		cjpeg.in_color_space = JCS_RGB;
	}
	else
	{
		cjpeg.input_components = 1;
		cjpeg.in_color_space = JCS_GRAYSCALE;
	}

	jpeg_set_defaults (&cjpeg);

	jpeg_simple_progression (&cjpeg);
	jpeg_set_quality (&cjpeg, quality, TRUE);
	cjpeg.dct_method = JDCT_FASTEST;
	jpeg_stdio_dest (&cjpeg,output);

	jpeg_start_compress (&cjpeg, TRUE);

	if (color) 
	{
		row_ptr[0] = line;
		for ( y = 0; y < height; y++)
		{
			for (x = 0; x < line_width; x += 3) 
			{
				line[x]   = image[x+2];
				line[x+1] = image[x+1];
				line[x+2] = image[x];
			}
			image += line_width;
			jpeg_write_scanlines (&cjpeg, row_ptr, 1);
		}
		free (line);
	}
	else 
	{
		for ( y = 0; y < height; y++) 
		{
			row_ptr[0] = image;
			jpeg_write_scanlines (&cjpeg, row_ptr, 1);
			image += line_width;
		}
	}
	jpeg_finish_compress (&cjpeg);
	jpeg_destroy_compress (&cjpeg);
}


