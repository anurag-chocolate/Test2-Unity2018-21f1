//
//  MediaLibraryHandler.m
//  Cross Platform Native Plugins
//
//  Created by Ashwin kumar on 10/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "MediaLibraryHandler.h"
#import "UIImage+Save.h"
#import "UIHandler.h"
#import "NSURL+Extensions.h"

@implementation MediaLibraryHandler

#define kPickImageFinished 				"PickImageFinished"
#define kSaveImageToGalleryFinished 	"SaveImageToGalleryFinished"
#define kPickVideoFinished 				"PickVideoFinished"
#define kPlayVideoFinished	 			"PlayVideoFinished"

@synthesize scaleFactor;
@synthesize allowsFullScreenVideoPlayback;
@synthesize allowsImageEditing;
@synthesize popoverController;
@synthesize moviePlayerVC;
@synthesize embeddedPlayerVC;

- (id)init
{
	self	= [super init];
	
	if (self)
	{
		// Add observer to get notified when application enters foreground
		[[NSNotificationCenter defaultCenter] addObserver:self
												 selector:@selector(willEnterForegroundNotification:)
													 name:UIApplicationWillEnterForegroundNotification
												   object:nil];
	}
	
	return self;
}

- (void)dealloc
{
	// Deregister from notification
	[[NSNotificationCenter defaultCenter] removeObserver:self];
	
	// Release
	self.popoverController 	= NULL;
	
	[super dealloc];
}

#pragma mark - Image Methods

- (BOOL)isCameraSupported
{
	bool isSupported 		= [UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypeCamera];
	NSLog(@"[MediaLibraryHandler] isCameraSupported: %d", isSupported);
	
	return isSupported;
}

#define kChooseExistingPhoto	@"Choose existing photo"
#define kCapturePhoto			@"Capture photo"

- (void)pickImage:(eImageSource)source scaleDownTo:(float)factor
{
	UIImagePickerControllerSourceType albumSource	= UIImagePickerControllerSourceTypePhotoLibrary;
	NSArray *imageType								= @[(NSString *)kUTTypeImage,
														(NSString *)kUTTypeJPEG,
														(NSString *)kUTTypeJPEG2000,
														(NSString *)kUTTypePNG];
	
    // Cache scale factor
    [self setScaleFactor:factor];
    
    // Camera is not supported
    if (![self isCameraSupported])
    {
		[self presentImagePickerForSource:albumSource
					 supportingMediaTypes:imageType
						   withEditOption:self.allowsImageEditing];
		
        return;
    }
    else
    {
        if (source == ALBUM)
        {
            [self presentImagePickerForSource:albumSource
						 supportingMediaTypes:imageType
							   withEditOption:self.allowsImageEditing];
			
            return;
        }
        else if (source == CAMERA)
        {
            [self presentImagePickerForSource:UIImagePickerControllerSourceTypeCamera
						 supportingMediaTypes:imageType
							   withEditOption:self.allowsImageEditing];
			
            return;
        }
        else
        {
			HybridActionSheet *actionsheet	= [[HybridActionSheet alloc] init];
			
			// Set properties
			[actionsheet setOtherButtons:@[kChooseExistingPhoto,
										   kCapturePhoto
										   ]];
			[actionsheet setDelegate:self];
			[actionsheet setSourceRect:[self getPopOverRect]];
			
			// Present
			[actionsheet presentFromViewController:UnityGetGLViewController()
										  animated:YES
										completion:nil];
        }
    }
}

- (void)presentImagePickerForSource:(UIImagePickerControllerSourceType)pickerSource
			   supportingMediaTypes:(NSArray *)mediaTypes
					 withEditOption:(BOOL)canEdit
{
	NSLog(@"[MediaLibraryHandler] image source: %d", pickerSource);
	
    UIImagePickerController *imagePickerController  = [[[UIImagePickerController alloc] init] autorelease];
    imagePickerController.delegate                  = self;
	imagePickerController.allowsEditing             = canEdit;
    imagePickerController.sourceType                = pickerSource;
    imagePickerController.mediaTypes                = mediaTypes;
	
    if (IsIpadInterface() && pickerSource != UIImagePickerControllerSourceTypeCamera)
    {
        self.popoverController						= [[[UIPopoverController alloc] initWithContentViewController:imagePickerController] autorelease];
		
		// Set properties
		[self.popoverController setDelegate:self];
		
        // Present it
        [self.popoverController presentPopoverFromRect:[self getPopOverRect]
                                                inView:UnityGetGLView()
                              permittedArrowDirections:UIPopoverArrowDirectionAny
                                              animated:YES];
    }
    else
    {
        [UnityGetGLViewController() presentViewController:imagePickerController
                                                 animated:YES
                                               completion:nil];
    }
}

#define kSelectedImagePath			@"image-path"
#define kPickImageFinishReason		@"finish-reason"

- (void)onDidFinishPickingImage:(UIImagePickerController *)picker withInfo:(NSDictionary *)info
{
	NSLog(@"[MediaLibraryHandler] finished picking image");
	
	// Fetch image
    UIImage *selectedImg	= [info objectForKey:UIImagePickerControllerEditedImage];
    
    // If edited image is null then use original
    if (selectedImg == NULL)
        selectedImg			= [info objectForKey:UIImagePickerControllerOriginalImage];
    
	NSString *imagePath  	= [selectedImg saveImageToDocumentsDirectory:@"NewlyPickedImage"];
	
    // Notify unity
	NSMutableDictionary *resultDict			= [NSMutableDictionary dictionary];
	
	if (imagePath)
	{
		resultDict[kSelectedImagePath]		= imagePath;
		resultDict[kPickImageFinishReason]	= [NSNumber numberWithInt:ePickImageFinishReasonSelected];
	}
	else
	{
		resultDict[kPickImageFinishReason]	= [NSNumber numberWithInt:ePickImageFinishReasonFailed];
	}
	
    NotifyEventListener(kPickImageFinished, ToJsonCString(resultDict));
}

- (void)onDidCancelPickImage
{
	NSLog(@"[MediaLibraryHandler] cancelled picking image");
	
	// Notify unity
	NSMutableDictionary *resultDict		= [NSMutableDictionary dictionary];
	resultDict[kPickImageFinishReason]	= [NSNumber numberWithInt:ePickImageFinishReasonCancelled];
	
	NotifyEventListener(kPickImageFinished, ToJsonCString(resultDict));
}

- (void)saveImageToGallery:(UIImage *)image
{
	NSLog(@"[MediaLibraryHandler] saving image to gallery");
	
	// Save to album
    UIImageWriteToSavedPhotosAlbum(image,
                                   self,
                                   @selector(image:didFinishSavingWithError:contextInfo:),
                                   nil);
}

- (void)               image:(UIImage *)image
    didFinishSavingWithError:(NSError *)error
                 contextInfo:(void *)contextInfo
{
    NSString *status = NULL;
    
    if (error != NULL)
        status  = @"false";
    else
        status  = @"true";
    
    // Notify Unity
    NotifyEventListener(kSaveImageToGalleryFinished, [status UTF8String]);
	NSLog(@"[MediaLibraryHandler] did finish saving, status: %@", status);
}

#pragma mark - Video Methods

- (void)playVideoUsingWebView:(NSString *)embedHTML
{
	// Stop playing video
	[self stopPlayingVideo];
	
	// Create instance
	EmbeddedVideoPlayerViewController *videoPlayerVC	= [[[EmbeddedVideoPlayerViewController alloc] init] autorelease];
	EmbeddedVideoPlayer *videoPlayer	= videoPlayerVC.videoPlayer;
	
	// Cache reference
	self.embeddedPlayerVC		= videoPlayerVC;
	
	// Set properties
	[videoPlayerVC setDelegate:self];
	[videoPlayer setEmbeddedHTMLString:embedHTML];
	
	// Play
	[videoPlayer play];
	
	// Present it
	[UnityGetGLViewController() presentViewController:videoPlayerVC
											 animated:YES
										   completion:nil];
}

- (void)playVideoFromURL:(NSString *)URLString fullscreen:(BOOL)fullscreen
{
	// Get URl
	NSURL *moviePathURL	= [NSURL createURLWithString:URLString];

	// Play video
	[self playVideoAtURL:moviePathURL fullscreen:fullscreen];
}

- (void)playVideoFromGallery:(BOOL)fullscreen
{
	NSLog(@"[MediaLibraryHandler] pick video from gallery");
	
	// Cache video playback mode
	[self setAllowsFullScreenVideoPlayback:fullscreen];
	
	// Present controller for selecting video
	[self presentImagePickerForSource:UIImagePickerControllerSourceTypePhotoLibrary
				 supportingMediaTypes:@[(NSString *)kUTTypeMovie]
					   withEditOption:NO];
}

- (void)onDidFinishPickingVideo:(UIImagePickerController *)picker withInfo:(NSDictionary *)info
{
	NSLog(@"[MediaLibraryHandler] finished picking video");
	NSURL *moviePathURL = [info objectForKey:UIImagePickerControllerMediaURL];
	
	// Notify unity
	NSString *pickVideoFinishReason	= [NSString stringWithFormat:@"%d", ePickVideoFinishReasonSelected];
    NotifyEventListener(kPickVideoFinished, [pickVideoFinishReason UTF8String]);
	
	// Play video
	[self playVideoAtURL:moviePathURL fullscreen:self.allowsFullScreenVideoPlayback];
}

- (void)onDidCancelPickVideo
{
	NSLog(@"[MediaLibraryHandler] cancelled picking video");
	
	// Notify unity
	NSString *pickVideoFinishReason	= [NSString stringWithFormat:@"%d", ePickVideoFinishReasonCancelled];
	
	NotifyEventListener(kPickVideoFinished, [pickVideoFinishReason UTF8String]);
}

- (BOOL)isVideoType:(NSArray *)mediaTypes
{
	NSString *mediaType = [mediaTypes objectAtIndex:0];
	
    return (CFStringCompare((CFStringRef) mediaType, kUTTypeMovie, 0) == kCFCompareEqualTo);
}

- (void)playVideoAtURL:(NSURL *)URL fullscreen:(BOOL)fullscreen
{
	NSLog(@"[MediaLibraryHandler] play video, URL=%@", [URL absoluteString]);
	
	// Stop playing video
	[self stopPlayingVideo];
	
	// Start creating instance
	self.moviePlayerVC 						= [[[MPMoviePlayerViewController alloc] initWithContentURL:URL] autorelease];
	MPMoviePlayerController *moviePlayer	= [moviePlayerVC moviePlayer];
	
	// Register for the movie player notification
	[[NSNotificationCenter defaultCenter] addObserver:self
											 selector:@selector(moviePlayerDidFinishReason:)
												 name:MPMoviePlayerPlaybackDidFinishNotification
											   object:nil];
	
	// Unregister to avoid stopping playback once in background
	[[NSNotificationCenter defaultCenter] removeObserver:self.moviePlayerVC
													name:UIApplicationDidEnterBackgroundNotification
												  object:nil];
		
	// Set properties
	[moviePlayer setControlStyle:MPMovieControlStyleFullscreen];
	[moviePlayer setScalingMode:MPMovieScalingModeAspectFit];
	[moviePlayer setFullscreen:fullscreen animated:NO];
	[moviePlayer setShouldAutoplay:YES];
	[moviePlayer prepareToPlay];
	
	// Present it
	[UnityGetGLViewController() presentMoviePlayerViewControllerAnimated:moviePlayerVC];
}

- (void)stopPlayingVideo
{
	// Stops movie player
	if (self.moviePlayerVC != nil)
	{
		[[self.moviePlayerVC moviePlayer] stop];
	}
	
	// Stops webview embedded player
	if (self.embeddedPlayerVC != nil)
	{
		[[self.embeddedPlayerVC videoPlayer] stop];
	}
}

#pragma mark - Image Picker Controller Delegate Methods

- (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary *)info
{
    NSLog(@"[MediaLibraryHandler] did finish picking");
	
	// Dismiss picker
	[UnityGetGLViewController() dismissViewControllerAnimated:YES
												   completion:^{
													   
													   // Release popover
													   if (self.popoverController)
													   {
														   [self.popoverController dismissPopoverAnimated:NO];
														   self.popoverController = nil;
													   }
													   
													   // Invoke appropriate handler
													   BOOL isVideoType	= [self isVideoType:[picker mediaTypes]];

													   if (isVideoType)
														   [self onDidFinishPickingVideo:picker withInfo:info];
													   else
														   [self onDidFinishPickingImage:picker withInfo:info];
												   }];
}

- (void)imagePickerControllerDidCancel:(UIImagePickerController *)picker
{
    NSLog(@"[MediaLibraryHandler] did cancel");
	
	// Dismiss picker
	[UnityGetGLViewController() dismissViewControllerAnimated:YES
												   completion:^{
													   
													   // Invoke cancel action handler
													   [self onImagePickerSelectionFailed:picker];
												   }];
}

- (void)onImagePickerSelectionFailed:(UIImagePickerController *)picker
{
	// Release popover
	if (self.popoverController)
	{
		[self.popoverController dismissPopoverAnimated:NO];
		self.popoverController = nil;
	}
	
	// Invoke appropriate handler
	if ([self isVideoType:[picker mediaTypes]])
		[self onDidCancelPickVideo];
	else
		[self onDidCancelPickImage];
}

#pragma mark - Popover Controller Delegate Methods

- (void)popoverControllerDidDismissPopover:(UIPopoverController *)popover
{
	NSLog(@"[MediaLibraryHandler] DidDismissPopover");
	
	[self onImagePickerSelectionFailed:(UIImagePickerController *)[popover contentViewController]];
}

#pragma mark - Action Sheet Delegate Methods

- (void)actionSheet:(HybridActionSheet *)actionSheet clickedButton:(NSString *)button;
{
	// Release
	[actionSheet release];

	// Execute appropriate pressed button action
	if (button)
	{
		if ([button isEqualToString:kChooseExistingPhoto])
		{
			[self pickImage:ALBUM scaleDownTo:self.scaleFactor];
			
			return;
		}
		else if ([button isEqualToString:kCapturePhoto])
		{
			[self pickImage:CAMERA scaleDownTo:self.scaleFactor];
			
			return;
		}
	}
	
	[self onDidCancelPickImage];
}

#pragma mark - UIApplication Notification Methods

- (void)willEnterForegroundNotification:(NSNotification *)notification
{
	// Resume video
	if (self.moviePlayerVC != nil)
	{
		if ([[self.moviePlayerVC moviePlayer] playbackState] == MPMoviePlaybackStatePaused)
		{
			[[self.moviePlayerVC moviePlayer] play];
		}
	}
	
	if (self.embeddedPlayerVC != nil)
	{
		if ([[self.embeddedPlayerVC videoPlayer] playbackState] == MPMoviePlaybackStatePaused)
		{
			[[self.embeddedPlayerVC videoPlayer] play];
		}
	}
}

#pragma mark - Movie Player Delegate Methods

- (void)moviePlayerDidFinishReason:(NSNotification *)notification
{
    NSLog(@"[MediaLibraryHandler] did finish playing video");
    MPMovieFinishReason reason	= [[[notification userInfo] valueForKey:MPMoviePlayerPlaybackDidFinishReasonUserInfoKey] intValue];
	
	// Notify unity
	NSString *reasonStr			= [NSString stringWithFormat:@"%d", reason];
	
	NotifyEventListener(kPlayVideoFinished, [reasonStr UTF8String]);
	
	// Remove as observer
    [[NSNotificationCenter defaultCenter] removeObserver:self
													name:MPMoviePlayerPlaybackDidFinishNotification
												  object:nil];
	
	// Dismiss and unset instance
	[UnityGetGLViewController() dismissMoviePlayerViewControllerAnimated];
	[self setMoviePlayerVC:nil];
}

#pragma mark - Embedded Player Delegate Methods

- (void)embeddedVideoPlayerViewController:(EmbeddedVideoPlayerViewController *)controller didFinishPlaying:(MPMovieFinishReason)reason
{
	// Notify unity
	NSString *reasonStr		= [NSString stringWithFormat:@"%d", reason];
	
	NotifyEventListener(kPlayVideoFinished, [reasonStr UTF8String]);
	
	// Dismiss and unset instance
	[UnityGetGLViewController() dismissViewControllerAnimated:NO
												   completion:nil];
	[self setEmbeddedPlayerVC:nil];
}

#pragma mark - Misc Methods

- (CGRect)getPopOverRect
{
	CGRect popoverRect;
	popoverRect.origin		= [[UIHandler Instance] popoverPoint];
	popoverRect.size		= CGSizeMake(1, 1);
	
	return popoverRect;
}

@end