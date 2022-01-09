using System;
using System.Collections.Generic;
using System.Text;

namespace HSPI_ENVIRACOM_MANAGER
{
	class HomeSeer
	{
        public enum UserRight
        {
            User_Guest = 1,
            User_Admin = 2,
            User_Local = 4,
            User_Normal = 8,
            User_Guest_Local = 5,
            User_Admin_Local = 6,
            User_Normal_Local = 12,
            User_Invalid = -1
        };

        public class HSUser
        {
            public string UserName;
            public UserRight Rights;
        };


		#region "  HomeSeer - Constants   "
		// User access levels
		public const int USER_GUEST =			(int) UserRight.User_Guest;
		public const int USER_ADMIN =			(int) UserRight.User_Admin;
		public const int USER_LOCAL =			(int) UserRight.User_Local;
		public const int USER_NORMAL =			(int) UserRight.User_Normal;

		//  TriggerUI control types
		public const int TRIG_UI_DROP_LIST =	1;
		public const int TRIG_UI_TEXT =			2;
		public const int TRIG_UI_CHECK_BOX =	3;
		public const int TRIG_UI_BUTTON =		4;
		public const int TRIG_UI_LABEL =		5;

		//  Plug-in capbailities
		public const int CA_X10 =				1;
		public const int CA_IR =				2;
		public const int CA_IO =				4;
		public const int CA_THERM =				16;

		//  Device MISC bit settings
		public const int MISC_PRESET_DIM  =		1;		// supports preset dim if set
		public const int MISC_EXT_DIM  =		2;      // extended dim command
		public const int MISC_SMART_LINC  =		4;      // smart linc switch
		public const int MISC_NO_LOG  =			8;      // no logging to event log for this device
		public const int MISC_STATUS_ONLY  =	0x10;   // device cannot be controlled
		public const int MISC_HIDDEN  =			0x20;   // device is hidden from views
		public const int MISC_THERM  =			0x40;   // device is a thermostat. Copied from dev attr
		public const int MISC_INCLUDE_PF  =		0x80;   // if set, device's state is restored if power fail enabled
		public const int MISC_SHOW_VALUES  =	0x100;  // set=display value options in win gui and web status
		public const int MISC_AUTO_VC  =		0x200;  // set=create a voice command for this device
		public const int MISC_VC_CONFIRM  =		0x400;  // set=confirm voice command
		public const int MISC_COMPOSE  =		0x800;  // compose protocol
		public const int MISC_ZWAVE  =			0x1000; // zwave device
		public const int MISC_DIRECT_DIM  =		0x2000; // Device supports direct dimming.
		// for compatibility with 1.7, the following 2 bits are 0
		// by default which disables SetDeviceStatus notify
		// and SetDeviceValue notify
		public const int MISC_SETSTATUS_NOTIFY =0x4000; // if set, SetDeviceStatus calls plugin SetIO
		// (default is 0 or not to notify)
		public const int MISC_SETVALUE_NOTIFY = 0x8000; // if set, SetDeviceValue calls plugin SetIO 
		// (default is 0 or to not notify)
		public const int MISC_NO_STATUS_TRIG  =	0x20000;// if set, the device will not appear in the device status
		// change trigger list or the device conditions list.

		// Device IOTYPE property bit settings
		public const int IOTYPE_INPUT  =		0;
		public const int IOTYPE_OUTPUT  =		1;
		public const int IOTYPE_ANALOG_INPUT =	2;
		public const int IOTYPE_VARIABLE  =		3;
		public const int IOTYPE_CONTROL  =		4;		// device is a control device, no type display in device 

		//  HSEvent callback types used with RegisterEventCB
		public const int EV_TYPE_X10  =				1;
		public const int EV_TYPE_LOG  =				2;
		public const int EV_TYPE_STATUS_CHANGE =	4;
		public const int EV_TYPE_AUDIO  =			8;
		public const int EV_TYPE_X10_TRANSMIT =		0x10;
		public const int EV_TYPE_CONFIG_CHANGE =	0x20;
		public const int EV_TYPE_STRING_CHANGE  =	0x40;
		public const int EV_TYPE_SPEAKER_CONNECT =	0x80;
		public const int EV_TYPE_CALLER_ID  =		0x100;
		public const int EV_TYPE_ZWAVE  =			0x200;	// Allows plugins to be notified of changes from a 
		                                                    // zwave device, such as the wakeup from a motion 
		                                                    // sensor. It will allow plugins to handle special 
		                                                    // stuff that HS does not handle.

		//  Event trigger Type enum
		public const int TYPE_TIME =			0;
		public const int TYPE_SUNRISE =			1;
		public const int TYPE_SUNSET =			2;
		public const int TYPE_X10 =				3;
		public const int TYPE_CONDITION =		4;
		public const int TYPE_POP_MAIL =		5;
		public const int TYPE_RECURRING =		6;
		public const int TYPE_MANUAL =			7;
		public const int TYPE_IRMATCH =			8;
		public const int TYPE_HVAC =		9;
		public const int TYPE_PHONE =			10;
		public const int TYPE_STATUS_CHANGE =	11;
		public const int TYPE_VALUE_CHANGE =	12;
		public const int TYPE_PLUGIN =			13;

		//  Value change trigger types enum
		public const int VALUE_EQUAL =			0;
		public const int VALUE_NOT_EQUAL =		1;
		public const int VALUE_GREATER =		2;
		public const int VALUE_LESS =			3;
		public const int VALUE_ANY =			4;
		public const int VALUE_SET_TO =			5;
		public const int VALUE_SET_TO_ANY =		6;
		public const int VALUE_IN_RANGE =		7;

		//  Device command enum
		public const int All_Unitm_Off =		0;
		public const int All_Lightm_On =		1;
		public const int UOn =					2;
		public const int UOff =					3;
		public const int UDim =					4;
		public const int UBright =				5;
		public const int All_Lightm_Off =		6;
		public const int Extended_Code =		7;
		public const int Hail_Request =			8;
		public const int HAil_Ack =				9;
		public const int Preset_Dim1 =			10;
		public const int Preset_Dim2 =			11;
		public const int Ex_Data_Xfer =			12;
		public const int Statum_On =			13;
		public const int Statum_Off =			14;
		public const int Statum_Request =		15;
		public const int Dim_To_Off =			16;
		public const int No_Cmd =				17;
		public const int Stat_Unknown =			17;
		public const int Any_Cmd =				18;
		public const int Value_Set =			19;
		public const int Value_Increment =		20;
		public const int Set_On =				21;
		public const int Set_Off =				22;
		public const int Set_Any =				23;
		public const int Value_Decrement =		24;

		//	InterfaceStatus returns
		public const short IS_ERR_NONE =		0;
		public const short IS_ERR_SEND =		1;
		public const short IS_ERR_INIT =		2;

		//	Access Level
		public const int AL_FREE =				1;
		public const int AL_LICENSED =			2;
		#endregion
	}
}
