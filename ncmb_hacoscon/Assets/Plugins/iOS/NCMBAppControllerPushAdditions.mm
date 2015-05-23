// For UnityAppController
#import "UnityAppController.h"
#import "UnityAppController+ViewHandling.h"
#import "UnityAppController+Rendering.h"
#import "iPhone_Sensors.h"

#import <CoreGraphics/CoreGraphics.h>
#import <QuartzCore/QuartzCore.h>
#import <QuartzCore/CADisplayLink.h>
#import <UIKit/UIKit.h>
#import <Availability.h>

#import <OpenGLES/EAGL.h>
#import <OpenGLES/EAGLDrawable.h>
#import <OpenGLES/ES2/gl.h>
#import <OpenGLES/ES2/glext.h>

#include "CrashReporter.h"
#include "iPhone_OrientationSupport.h"
#include "iPhone_Profiler.h"

#include "UI/Keyboard.h"
#include "UI/UnityView.h"
#include "UI/SplashScreen.h"
#include "Unity/DisplayManager.h"
#include "Unity/EAGLContextHelper.h"
#include "Unity/GlesHelper.h"
#include "PluginBase/AppDelegateListener.h"

// For Push
#import <NCMB/NCMB.h>
#include <stdio.h>

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Unity default methods
void UnitySendMessage( const char * className, const char * methodName, const char * param );
void UnitySendDeviceToken( NSData* deviceToken );
void UnitySendRemoteNotification( NSDictionary* notification );
void UnitySendRemoteNotificationError( NSError* error );
void UnitySendLocalNotification( UILocalNotification* notification );

// Notify & Log methods
void notifyUnity(const char * method, const char * message)
{
    UnitySendMessage("NCMBManager", method, message);
    //NSLog(@"[NCMB] %s : %s", method, message);
}

void notifyUnityError(const char * method, NSError * error)
{
    const char* message = [[error description] UTF8String];
    notifyUnity(method, message);
    //NSLog(@"[NCMB] %s : %s", method, message);
}

// Native code
extern "C"
{
    // Use location or not
    bool getLocation;
    
    // Save launch options for using later (after set key)
    NSDictionary * savedLaunchOptions;
    
    void afterLaunch()
    {
        // NCMB Track
        [NCMBAnalytics trackAppOpenedWithLaunchOptions:savedLaunchOptions];
        
        // NCMB Handle Rich Push
        [NCMBPush handleRichPush:[savedLaunchOptions objectForKey:@"UIApplicationLaunchOptionsRemoteNotificationKey"]];
    }
    
    void saveInstallation(NCMBInstallation * currentInstallation)
    {
        [currentInstallation saveInBackgroundWithBlock:^(BOOL succeeded, NSError *error) {
            if(succeeded){
                //端末情報の登録が成功した場合の処理
                notifyUnity("OnRegistrationSucceeded", "");
                
                afterLaunch();
            } else {
                //端末情報の登録が失敗した場合の処理
                if (error.code == 409){
                    //失敗理由がdeviceTokenの重複だった場合は、登録された端末情報を取得する
                    NCMBQuery *installationQuery = [NCMBInstallation query];
                    [installationQuery whereKey:@"deviceToken" equalTo:currentInstallation.deviceToken];
                    
                    NSError *searchErr = nil;
                    NCMBInstallation *searchDevice = (NCMBInstallation*)[installationQuery getFirstObject:&searchErr];
                    
                    if (!searchErr){
                        //上書き保存する
                        currentInstallation.objectId = searchDevice.objectId;
                        [currentInstallation saveInBackgroundWithBlock:^(BOOL succeeded, NSError *error) {
                            if (error){
                                //端末情報更新に失敗したときの処理
                                notifyUnityError("OnRegistrationFailed", error);
                            } else {
                                //端末情報更新に成功したときの処理
                                notifyUnity("OnRegistrationSucceeded", "");
                                
                                afterLaunch();
                            }
                        }];
                    } else {
                        notifyUnity("OnRegistrationFailed", "Can't get first object from Installation Class.");
                    }
                } else {
                    notifyUnityError("OnRegistrationFailed", error);
                }
            }
        }];
    }
    
    void initialize(const char * applicationKey, const char * clientKey)
    {
        [NCMB setApplicationKey:GetStringParam(applicationKey) clientKey:GetStringParam(clientKey)];
    }
    
    void registerCommon()
    {
        if (NSFoundationVersionNumber > NSFoundationVersionNumber_iOS_7_1){
            UIUserNotificationType type = UIRemoteNotificationTypeAlert |
            UIRemoteNotificationTypeBadge |
            UIRemoteNotificationTypeSound;
            UIUserNotificationSettings *setting = [UIUserNotificationSettings settingsForTypes:type
                                                                                    categories:nil];
            [[UIApplication sharedApplication] registerUserNotificationSettings:setting];
            [[UIApplication sharedApplication] registerForRemoteNotifications];
        } else {
            [[UIApplication sharedApplication] registerForRemoteNotificationTypes:
             (UIRemoteNotificationTypeAlert |
              UIRemoteNotificationTypeBadge |
              UIRemoteNotificationTypeSound)];
        }
        
    }
    
    void registerNotification()
    {
        getLocation = false;
        registerCommon();
    }
    
    void registerNotificationWithLocation()
    {
        getLocation = true;
        registerCommon();
    }
    
    void changeKey(NSMutableDictionary *data, NSString *oldKey, NSString *newKey)
    {
        // Check exist key
        id obj = [data objectForKey:oldKey];
        
        if (obj)
        {
            // Add newKey to data
            NSNumber *nObj = [NSNumber numberWithBool:(bool)obj];
            [data setObject:nObj forKey:newKey];
            
            // Remove oldKey from data
            [data removeObjectForKey:oldKey];
        }
    }
    
    void sendPush(const char * json, const char * message, int delayByMilliseconds, bool dialog)
    {
        NCMBPush *push = [[NCMBPush alloc] init];
        
        // Json to Dictionary
        NSString *nsJson = GetStringParam(json);
        
        // Data
        NSError *error = nil;
        NSData *jsonData = [nsJson dataUsingEncoding:NSUTF8StringEncoding];
        NSMutableDictionary *data = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONReadingMutableContainers error:&error];
        
        // Replace badge key to badgeIncrementFlag, convert bool object to number
        changeKey(data, @"badge", @"badgeIncrementFlag");
        
        // Replace content-available key to contentAvailable, convert bool object to number
        changeKey(data, @"content-available", @"contentAvailable");
        
        //default set
        NSArray *target = [data objectForKey:@"target"];
        if([target indexOfObject:@"ios"] != NSNotFound || target.count == 2 || target == nil){
            data[@"sound"] = @"default";
        }
        
        // Set data and message
        [push setData:data];
        [push setMessage:GetStringParam(message)];
        
        // Dialog
        if (dialog)
        {
            [push setDialog:true];
        }
        
        // Immediate or delay
        if (delayByMilliseconds == 0)
        {
            [push setImmediateDeliveryFlag:true];
        }
        else
        {
            [push setDeliveryTime:[NSDate dateWithTimeIntervalSinceNow:((double)delayByMilliseconds / 1000)]];
        }
        
        // Send
        [push sendPushInBackgroundWithBlock:^(BOOL succeeded, NSError *error) {
            if (error)
            {
                notifyUnityError("OnSendPushFailed", error);
            }
            else
            {
                notifyUnity("OnSendPushSucceeded", "");
            }
        }];
    }
    
    void clearAll()
    {
        [[UIApplication sharedApplication] setApplicationIconBadgeNumber: 0];
    }
}

// Implementation
#if UNITY_VERSION < 420

@implementation AppController(PushAdditions)

#else

@implementation UnityAppController(PushAdditions)

#endif

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark UIApplicationDelegate

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    savedLaunchOptions = [launchOptions copy];
    
    printf_console("-> applicationDidFinishLaunching()\n");
    // get local notification
    if (&UIApplicationLaunchOptionsLocalNotificationKey != nil)
    {
        UILocalNotification *notification = [launchOptions objectForKey:UIApplicationLaunchOptionsLocalNotificationKey];
        if (notification)
            UnitySendLocalNotification(notification);
    }
    
    // get remote notification
    if (&UIApplicationLaunchOptionsRemoteNotificationKey != nil)
    {
        NSDictionary *notification = [launchOptions objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey];
        if (notification)
            UnitySendRemoteNotification(notification);
    }
    
    if ([UIDevice currentDevice].generatesDeviceOrientationNotifications == NO)
        [[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];
    
    [DisplayManager Initialize];
    
    _mainDisplay	= [[[DisplayManager Instance] mainDisplay] createView:YES showRightAway:NO];
    _window			= _mainDisplay->window;
    
    [KeyboardDelegate Initialize];
    
    [self createViewHierarchy];
    [self preStartUnity];
    UnityInitApplicationNoGraphics([[[NSBundle mainBundle] bundlePath]UTF8String]);
    
    return YES;
}

- (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken
{
    if (getLocation)
    {
        //iOS8の場合、位置情報の利用をリクエストする
        //別途Info.plistの編集も必要なので、SDKガイドの位置情報検索をご覧ください。
        if ([[[UIDevice currentDevice] systemVersion] floatValue] >= 8.0){
            CLLocationManager *locationManager = [[CLLocationManager alloc] init];
            [locationManager requestWhenInUseAuthorization];
        }
        
        //現在地を非同期処理で取得する
        [NCMBGeoPoint geoPointForCurrentLocationInBackground:^(NCMBGeoPoint *geoPoint, NSError *error) {
            if (error){
                //位置情報取得に失敗したエラー処理
                notifyUnityError("OnGetLocationFailed", error);
            } else {
                char sgeo[256];
                sprintf(sgeo, "%lf %lf", geoPoint.latitude, geoPoint.longitude);
                
                notifyUnity("OnGetLocationSucceeded", sgeo);
                
                //位置情報が取得できた場合の処理
                NCMBInstallation *installation = [NCMBInstallation currentInstallation];
                [installation setDeviceTokenFromData:deviceToken];
                [installation setObject:geoPoint forKey:@"geoPoint"];
                [installation setObject:[[UIDevice currentDevice] systemVersion] forKey:@"osVersion"];
                
                saveInstallation(installation);
            }
        }];
    }
    else
    {
        NCMBInstallation *installation = [NCMBInstallation currentInstallation];
        [installation setDeviceTokenFromData:deviceToken];
        [installation setObject:[[UIDevice currentDevice] systemVersion] forKey:@"osVersion"];
        
        saveInstallation(installation);
    }
    
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
    UnitySendRemoteNotificationError( error );
    
    notifyUnityError("OnRemoteRegistrationDidFail", error);
}


- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
    AppController_SendNotificationWithArg(kUnityDidReceiveRemoteNotification, userInfo);
    UnitySendRemoteNotification(userInfo);
    
    // NCMB Handle Rich Push
    if ([userInfo.allKeys containsObject:@"com.nifty.RichUrl"])
    {
        if ([[UIApplication sharedApplication] applicationState] != UIApplicationStateActive)
        {
            [NCMBPush handleRichPush:userInfo];
        }
    }
    
    // NCMB Track
    //if (application.applicationState != UIApplicationStateActive)
    //{
        [NCMBAnalytics trackAppOpenedWithRemoteNotificationPayload:userInfo];
    //}
}


@end