using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineOffset.WeatherStation
{
    enum ws_types { ub, sb, us, ss, dt, tt, pb, wa, wg, dp };

    class WSstruct
    {
        /* Fine Offset Weather Station Reader - Header file
        (C) Arne-Jørgen Auberg (arne.jorgen.auberg@gmail.com)
        - Wireless Weather Station Data Block Definition
        - Wireless Weather Station Record Format Definition
        - Wunderground Record Format
        - PYWWS Record Format
        - PWS Weather Record Format
        - CUSB class for open, initialize and close of USB interface
        - CWS class for open, read, and close of WS buffer
        - CWF class for write to selected log file format
        04.06.13 Josch parentheses for #defines to avoid side effects
        20.06.13 Josch Format similar to WS3600 (fetch3600) to use in FHEM
        19.08.13 Josch Dougs barometer correction from 27.09.12 included
        10.09.13 Josch Typ von CUSB_read_block() geaendert
        26.09.13 Josch Includes moved to fowsr.c, c99 style removed
        15.10.13 Josch output rel. pressure in ws3600_format
        */

        // Parameters used by the cache file
        public const int WS_CACHE_READ = 0;
        const int WS_CACHE_WRITE = 1;

        // Weather Station buffer parameters
        const int WS_BUFFER_SIZE = 0x10000;         // Size of total buffer
        const int WS_BUFFER_START = 0x100;          // First address of up to 4080 buffer records
        const int WS_BUFFER_END = 0xFFF0;           // Position of last buffer record
        const int WS_BUFFER_RECORD = 0x10;          // Size of one buffer record
        const int WS_BUFFER_CHUNK = 0x20;           // Size of chunk received over USB
        const int WS_FIXED_BLOCK_START = 0x0000;    // First address of fixed block
        const int WS_FIXED_BLOCK_SIZE = 0x0100;     // Size of fixed block

        // Weather Station buffer memory positions
        const int WS_DELAY = 0; // Position of delay parameter
        const int WS_HUMIDITY_IN = 1;   // Position of inside humidity parameter
        const int WS_TEMPERATURE_IN = 2;    // Position of inside temperature parameter
        public const int WS_HUMIDITY_OUT = 4;  // Position of outside humidity parameter
        public const int WS_TEMPERATURE_OUT = 5;   // Position of outside temperature parameter
        const int WS_ABS_PRESSURE = 7;  // Position of absolute pressure parameter
        const int WS_WIND_AVE = 9;  // Position of wind direction parameter
        const int WS_WIND_GUST = 10;    // Position of wind direction parameter
        const int WS_WIND_DIR = 12; // Position of wind direction parameter
        const int WS_RAIN = 13; // Position of rain parameter
        const int WS_STATUS = 15;   // Position of status parameter
        const int WS_DATA_COUNT = 27;   // Position of data_count parameter
        const int WS_CURRENT_POS = 30;  // Position of current_pos parameter
        const int WS_CURR_REL_PRESSURE = 32;    // Position of current relative pressure parameter
        const int WS_CURR_ABS_PRESSURE = 34;    // Position of current absolute pressure parameter

        // Calculated rain parameters
        // NOTE: These positions are NOT stored in the Weather Station
        const int WS_RAIN_HOUR = 0x08;  // Position of hourly calculated rain
        const int WS_RAIN_DAY = 0x0A;   // Position of daily calculated rain
        const int WS_RAIN_WEEK = 0x0C;  // Position of weekly calculated rain
        const int WS_RAIN_MONTH = 0x0E; // Position of monthly calculated rain


        // The following setting parameters are for reference only
        // A future user interface could interpret these parameters
        // Unit settings
        const int WS_UNIT_SETTING_IN_T_C_F = 0x01;
        const int WS_UNIT_SETTING_OUT_T_C_F = 0x02;
        const int WS_UNIT_SETTING_RAIN_FALL_CM_IN = 0x04;
        const int WS_UNIT_SETTING_PRESSURE_HPA = 0x20;
        const int WS_UNIT_SETTING_PRESSURE_INHG = 0x40;
        const int WS_UNIT_SETTING_PRESSURE_MMHG = 0x80;
        // Unit wind speed settings
        const int WS_UNIT_SETTING_WIND_SPEED_MS = 0x01;
        const int WS_UNIT_SETTING_WIND_SPEED_KMH = 0x02;
        const int WS_UNIT_SETTING_WIND_SPEED_KNOT = 0x04;
        const int WS_UNIT_SETTING_WIND_SPEED_MH = 0x08;
        const int WS_UNIT_SETTING_WIND_SPEED_BFT = 0x10;
        // Display format 0
        const int WS_DISPLAY_FORMAT_P_ABS_REL = 0x01;
        const int WS_DISPLAY_FORMAT_WSP_AVG_GUST = 0x02;
        const int WS_DISPLAY_FORMAT_H_24_12 = 0x04;
        const int WS_DISPLAY_FORMAT_DDMMYY_MMDDYY = 0x08;
        const int WS_DISPLAY_FORMAT_TS_H_12_24 = 0x10;
        const int WS_DISPLAY_FORMAT_DATE_COMPLETE = 0x20;
        const int WS_DISPLAY_FORMAT_DATE_AND_WK = 0x40;
        const int WS_DISPLAY_FORMAT_ALARM_TIME = 0x80;
        // Display format 1
        const int WS_DISPLAY_FORMAT_OUT_T = 0x01;
        const int WS_DISPLAY_FORMAT_OUT_WINDCHILL = 0x02;
        const int WS_DISPLAY_FORMAT_OUT_DEW_POINT = 0x04;
        const int WS_DISPLAY_FORMAT_RAIN_FALL_1H = 0x08;
        const int WS_DISPLAY_FORMAT_RAIN_FALL_24H = 0x10;
        const int WS_DISPLAY_FORMAT_RAIN_FALL_WK = 0x20;
        const int WS_DISPLAY_FORMAT_RAIN_FALL_MO = 0x40;
        const int WS_DISPLAY_FORMAT_RAIN_FALL_TOT = 0x80;
        // Alarm enable 0
        const int WS_ALARM_ENABLE_TIME = 0x02;
        const int WS_ALARM_ENABLE_WIND_DIR = 0x04;
        const int WS_ALARM_ENABLE_IN_RH_LO = 0x10;
        const int WS_ALARM_ENABLE_IN_RH_HI = 0x20;
        const int WS_ALARM_ENABLE_OUT_RH_LO = 0x40;
        const int WS_ALARM_ENABLE_OUT_RH_HI = 0x80;
        // Alarm enable 1
        const int WS_ALARM_ENABLE_WSP_AVG = 0x01;
        const int WS_ALARM_ENABLE_WSP_GUST = 0x02;
        const int WS_ALARM_ENABLE_RAIN_FALL_1H = 0x04;
        const int WS_ALARM_ENABLE_RAIN_FALL_24H = 0x08;
        const int WS_ALARM_ENABLE_ABS_P_LO = 0x10;
        const int WS_ALARM_ENABLE_ABS_P_HI = 0x20;
        const int WS_ALARM_ENABLE_REL_P_LO = 0x40;
        const int WS_ALARM_ENABLE_REL_P_HI = 0x80;
        // Alarm enable 2
        const int WS_ALARM_ENABLE_IN_T_LO = 0x01;
        const int WS_ALARM_ENABLE_IN_T_HI = 0x02;
        const int WS_ALARM_ENABLE_OUT_T_LO = 0x04;
        const int WS_ALARM_ENABLE_OUT_T_HI = 0x08;
        const int WS_ALARM_ENABLE_WINDCHILL_LO = 0x10;
        const int WS_ALARM_ENABLE_WINDCHILL_HI = 0x20;
        const int WS_ALARM_ENABLE_DEWPOINT_LO = 0x40;
        const int WS_ALARM_ENABLE_DEWPOINT_HI = 0x80;


        // Conversion parameters for english units
        // Second and optional third factor is for adapting to actual stored values
        const double WS_SCALE_DEFAULT = 1.0;   // No scaling
        const double WS_SCALE_MS_TO_MPH = (2.2369362920544 * 0.1);
        const double WS_SCALE_C_TO_F = (1.8 * 0.1);
        const double WS_SCALE_CM_TO_IN = (0.3937007874 * 0.1 * 0.3);
        const double WS_SCALE_RAW_TO_inHg = (0.029530058646697 * 0.1);
        const double WS_SCALE_hPa_TO_inHg = (0.029530058646697);
        const double WS_SCALE_OFFS_TO_DEGREE = 22.5;

        const double WS_OFFSET_DEFAULT = 0.0;  // No offset
        const double WS_OFFSET_C_TO_F = 32.0;


        // Table for decoding raw weather station data.
        // Each key specifies a (pos, type, scale) tuple that is understood by CWS_decode().
        // See http://www.jim-easterbrook.me.uk/weather/mm/ for description of data



        struct ws_record
        {
            private string name;
            private int pos;
            private ws_types ws_type;
            private double scale;

            public ws_record(string name, int pos, ws_types wsType, double scale) : this()
            {
                this.name = name;
                this.pos = pos;
                this.ws_type = wsType;
                this.scale = scale;
            }
        };

        ws_record[] ws_format = new ws_record[]{
	        // Up to 4080 records with this format
	        new ws_record ("delay", 0, ws_types.ub, 1.0 ), // Minutes since last stored reading (1:240)
	        new ws_record ("hum_in", 1, ws_types.ub, 1.0 ), // Indoor relative humidity %        (1:99)    , 0xFF means invalid
	        new ws_record ("temp_in", 2, ws_types.ss, 0.1 ), // Multiply by 0.1 to get °C         (-40:+60) , 0xFFFF means invalid
	        new ws_record ("hum_out", 4, ws_types.ub, 1.0 ), // Outdoor relative humidity %       (1:99)    , 0xFF means invalid
	        new ws_record ("temp_out", 5, ws_types.ss, 0.1 ), // Multiply by 0.1 to get °C         (-40:+60) , 0xFFFF means invalid
	        new ws_record ("abs_pressure", 7, ws_types.us, 0.1 ), // Multiply by 0.1 to get hPa        (920:1080), 0xFFFF means invalid
	        new ws_record ("wind_ave", 9, ws_types.wa, 0.1 ), // Multiply by 0.1 to get m/s        (0:50)    , 0xFF means invalid
	        new ws_record ("wind_gust", 10, ws_types.wg, 0.1 ), // Multiply by 0.1 to get m/s        (0:50)    , 0xFF means invalid
	        // 11, wind speed, high bits     // Lower 4 bits are the average wind speed high bits, upper 4 bits are the gust wind speed high bits
	        new ws_record ( "wind_dir", 12, ws_types.ub, 22.5 ), // Multiply by 22.5 to get ° from north (0-15), 7th bit indicates invalid data
	        new ws_record ( "rain", 13, ws_types.us, 0.3 ), // Multiply by 0.3 to get mm
	        new ws_record ( "status", 15, ws_types.pb, 1.0 ), // 6th bit indicates loss of contact with sensors, 7th bit indicates rainfall overflow
	        // The lower fixed block
	        new ws_record ( "read_period", 16, ws_types.ub, 1.0 ), // Minutes between each stored reading (1:240)
	        new ws_record ( "units0", 17, ws_types.ub, 1.0 ), // Unit setting flags       (Bits 0,1,2,    5,6,7)
	        new ws_record ( "units_wind_speed", 18, ws_types.ub, 1.0 ), // Unit wind speed settings (Bits 0,1,2,3,4      )
	        new ws_record ( "display_format0", 19, ws_types.ub, 1.0 ), // Unit display settings    (Bits 0,1,2,3,4,5,6,7)
	        new ws_record ( "display_format1", 20, ws_types.ub, 1.0 ), // Unit display settings    (Bits 0,1,2,3,4,5,6,7)
	        new ws_record ( "alarm_enable0", 21, ws_types.ub, 1.0 ), // Unit alarm settings      (Bits   1,2,  4,5,6,7)
	        new ws_record ( "alarm_enable1", 22, ws_types.ub, 1.0 ), // Unit alarm settings      (Bits 0,1,2,3,4,5,6,7)
	        new ws_record ( "alarm_enable2", 23, ws_types.ub, 1.0 ), // Unit alarm settings      (Bits 0,1,2,3,4,5,6,7)
	        new ws_record ( "timezone", 24, ws_types.sb, 1.0 ), // Hours offset from Central European Time, so in the UK this should be set to -1. In stations without a radio controlled clock this is always zero. 7th bit is sign bit
	        new ws_record ( "data_refreshed", 26, ws_types.us, 1.0 ), // PC write AA indicating setting changed, base unit clear this byte for reading back the change
	        new ws_record ( "data_count", 27, ws_types.us, 1.0 ), // Number of stored readings. Starts at zero, rises to 4080
	        new ws_record ( "current_pos", 30, ws_types.us, 1.0 ), // Address of the stored reading currently being created. Starts at 256, rises to 65520 in steps of 16, then loops back to 256. The data at this address is updated every 48 seconds or so, until the read period is reached. Then the address is incremented and the next record becomes current.
	        new ws_record ( "rel_pressure", 32, ws_types.us, 0.1 ), // Current relative atmospheric pressure, multiply by 0.1 to get hPa
	        new ws_record ( "abs_pressure", 34, ws_types.us, 0.1 ), // Current absolute atmospheric pressure, multiply by 0.1 to get hPa
	        new ws_record ( "date_time", 43, ws_types.dt, 1.0 ), // Current date & time
	        // Alarm settings
	        new ws_record ( "alarm.hum_in.hi", 48, ws_types.ub, 1.0 ), new ws_record ( "alarm.hum_in.lo", 49, ws_types.ub, 1.0 ), // Indoor relative humidity %
	        new ws_record ( "alarm.temp_in.hi", 50, ws_types.ss, 0.1 ), new ws_record ( "alarm.temp_in.lo", 52, ws_types.ss, 0.1 ), // Multiply by 0.1 to get °C
	        new ws_record ( "alarm.hum_out.hi", 54, ws_types.ub, 1.0 ), new ws_record ( "alarm.hum_out.lo", 55, ws_types.ub, 1.0 ), // Indoor relative humidity %
	        new ws_record ( "alarm.temp_out.hi", 56, ws_types.ss, 0.1 ), new ws_record ( "alarm.temp_out.lo", 58, ws_types.ss, 0.1 ), // Multiply by 0.1 to get °C
	        new ws_record ( "alarm.windchill.hi", 60, ws_types.ss, 0.1 ), new ws_record ( "alarm.windchill.lo", 62, ws_types.ss, 0.1 ), // Multiply by 0.1 to get °C
	        new ws_record ( "alarm.dewpoint.hi", 64, ws_types.ss, 0.1 ), new ws_record ( "alarm.dewpoint.lo", 66, ws_types.ss, 0.1 ), // Multiply by 0.1 to get °C
	        new ws_record ( "alarm.abs_pressure.hi", 68, ws_types.ss, 0.1 ), new ws_record ( "alarm.abs_pressure.lo", 70, ws_types.ss, 0.1 ), // Multiply by 0.1 to get hPa
	        new ws_record ( "alarm.rel_pressure.hi", 72, ws_types.ss, 0.1 ), new ws_record ( "alarm.rel_pressure.lo", 74, ws_types.ss, 0.1 ), // Multiply by 0.1 to get hPa
	        new ws_record ( "alarm.wind_ave.bft", 76, ws_types.ub, 1.0 ), new ws_record ( "alarm.wind_ave.ms", 77, ws_types.ub, 0.1 ), // Multiply by 0.1 to get m/s
	        new ws_record ( "alarm.wind_gust.bft", 79, ws_types.ub, 1.0 ), new ws_record ( "alarm.wind_gust.ms", 80, ws_types.ub, 0.1 ), // Multiply by 0.1 to get m/s
	        new ws_record ( "alarm.wind_dir", 82, ws_types.ub, 22.5 ),                                          // Multiply by 22.5 to get ° from north
	        new ws_record ( "alarm.rain.hour", 83, ws_types.us, 0.3 ), new ws_record ( "alarm.rain.day", 85, ws_types.us, 0.3 ), // Multiply by 0.3 to get mm
	        new ws_record ( "alarm.time", 87, ws_types.tt, 1.0 ),
	        // Maximums with timestamps
	        new ws_record ( "max.hum_in.val", 98, ws_types.ub, 1.0 ), new ws_record ( "max.hum_in.date", 141, ws_types.dt, 1.0 ),
            new ws_record ( "max.hum_out.val", 100, ws_types.ub, 1.0 ), new ws_record ( "max.hum_out.date", 151, ws_types.dt, 1.0 ),
            new ws_record ( "max.temp_in.val", 102, ws_types.ss, 0.1 ), new ws_record ( "max.temp_in.date", 161, ws_types.dt, 1.0 ), // Multiply by 0.1 to get °C
	        new ws_record ( "max.temp_out.val", 106, ws_types.ss, 0.1 ), new ws_record ( "max.temp_out.date", 171, ws_types.dt, 1.0 ), // Multiply by 0.1 to get °C
	        new ws_record ( "max.windchill.val", 110, ws_types.ss, 0.1 ), new ws_record ( "max.windchill.date", 181, ws_types.dt, 1.0 ), // Multiply by 0.1 to get °C
	        new ws_record ( "max.dewpoint.val", 114, ws_types.ss, 0.1 ), new ws_record ( "max.dewpoint.date", 191, ws_types.dt, 1.0 ), // Multiply by 0.1 to get °C
	        new ws_record ( "max.abs_pressure.val", 118, ws_types.us, 0.1 ), new ws_record ( "max.abs_pressure.date", 201, ws_types.dt, 1.0 ), // Multiply by 0.1 to get hPa
	        new ws_record ( "max.rel_pressure.val", 122, ws_types.us, 0.1 ), new ws_record ( "max.rel_pressure.date", 211, ws_types.dt, 1.0 ), // Multiply by 0.1 to get hPa
	        new ws_record ( "max.wind_ave.val", 126, ws_types.us, 0.1 ), new ws_record ( "max.wind_ave.date", 221, ws_types.dt, 1.0 ), // Multiply by 0.1 to get m/s
	        new ws_record ( "max.wind_gust.val", 128, ws_types.us, 0.1 ), new ws_record ( "max.wind_gust.date", 226, ws_types.dt, 1.0 ), // Multiply by 0.1 to get m/s
	        new ws_record ( "max.rain.hour.val", 130, ws_types.us, 0.3 ), new ws_record ( "max.rain.hour.date", 231, ws_types.dt, 1.0 ), // Multiply by 0.3 to get mm
	        new ws_record ( "max.rain.day.val", 132, ws_types.us, 0.3 ), new ws_record ( "max.rain.day.date", 236, ws_types.dt, 1.0 ), // Multiply by 0.3 to get mm
	        new ws_record ( "max.rain.week.val", 134, ws_types.us, 0.3 ), new ws_record ( "max.rain.week.date", 241, ws_types.dt, 1.0 ), // Multiply by 0.3 to get mm
	        new ws_record ( "max.rain.month.val", 136, ws_types.us, 0.3 ), new ws_record ( "max.rain.month.date", 246, ws_types.dt, 1.0 ), // Multiply by 0.3 to get mm
	        new ws_record ( "max.rain.total.val", 138, ws_types.us, 0.3 ), new ws_record ( "max.rain.total.date", 251, ws_types.dt, 1.0 ), // Multiply by 0.3 to get mm
	        // Minimums with timestamps
	        new ws_record ( "min.hum_in.val", 99, ws_types.ub, 1.0 ), new ws_record ( "min.hum_in.date", 146, ws_types.dt, 1.0 ),
            new ws_record ( "min.hum_out.val", 101, ws_types.ub, 1.0 ), new ws_record ( "min.hum_out.date", 156, ws_types.dt, 1.0 ),
            new ws_record ( "min.temp_in.val", 104, ws_types.ss, 0.1 ), new ws_record ( "min.temp_in.date", 166, ws_types.dt, 1.0 ), // Multiply by 0.1 to get °C
	        new ws_record ( "min.temp_out.val", 108, ws_types.ss, 0.1 ), new ws_record ( "min.temp_out.date", 176, ws_types.dt, 1.0 ), // Multiply by 0.1 to get °C
	        new ws_record ( "min.windchill.val", 112, ws_types.ss, 0.1 ), new ws_record ( "min.windchill.date", 186, ws_types.dt, 1.0 ), // Multiply by 0.1 to get °C
	        new ws_record ( "min.dewpoint.val", 116, ws_types.ss, 0.1 ), new ws_record ( "min.dewpoint.date", 196, ws_types.dt, 1.0 ), // Multiply by 0.1 to get °C
	        new ws_record ( "min.abs_pressure.val", 120, ws_types.us, 0.1 ), new ws_record ( "min.abs_pressure.date", 206, ws_types.dt, 1.0 ), // Multiply by 0.1 to get hPa
	        new ws_record ( "min.rel_pressure.val", 124, ws_types.us, 0.1 ), new ws_record ( "min.rel_pressure.date", 216, ws_types.dt, 1.0 ), // Multiply by 0.1 to get hPa
	        // Calculated rainfall, must be calculated prior to every record
	        new ws_record ( "rain.hour", WS_RAIN_HOUR, ws_types.us, 0.3 ), // Multiply by 0.3 to get mm
	        new ws_record ( "rain.day", WS_RAIN_DAY, ws_types.us, 0.3 ), // Multiply by 0.3 to get mm
	        new ws_record ( "rain.week", WS_RAIN_WEEK, ws_types.us, 0.3 ), // Multiply by 0.3 to get mm
	        new ws_record ( "rain.month", WS_RAIN_MONTH, ws_types.us, 0.3 )  // Multiply by 0.3 to get mm
        };
    }

    class WSdecoder
    {
        int CWS_decode(byte[] raw, ws_types ws_type, float scale, float offset, out string result)
        {

            int b = Convert.ToInt32(-(Math.Log(scale) + 0.5));
            int i, m = 0, n = 0;
            float fresult;

            if (b < 1) b = 1;
            //if (!result) return 0;
            //else *result = '\0';
            result = "";
            switch (ws_type)
            {
                case ws_types.ub:
                    m = 1;
                    fresult = raw[0] * scale + offset;
                    //n = sprintf(result, "%.*f", b, fresult);
                    String.Format(result,"%.*f", b, fresult);
                    break;
                case ws_types.sb:
                    m = 1;
                    fresult = raw[0] & 0x7F;
                    if (Convert.ToBoolean(raw[0] & 0x80))  // Test for sign bit
                        fresult -= fresult; //negative value
                    fresult = fresult * scale + offset;
                    //n = sprintf(result, "%.*f", b, fresult);
                    String.Format(result, "%.*f", b, fresult);
                    break;
                case ws_types.us:
                    m = 2;
                    fresult = CWS_unsigned_short(raw) * scale + offset;
                    //n = sprintf(result, "%.*f", b, fresult);
                    String.Format(result, "%.*f", b, fresult);
                    break;
                case ws_types.ss:
                    m = 2;
                    fresult = CWS_signed_short(new ArraySegment<byte>(raw)) * scale + offset;
                    //n = sprintf(result, "%.*f", b, fresult);
                    String.Format(result, "%.*f", b, fresult);
                    break;
                case ws_types.dt:
                    {
                        char year, month, day, hour, minute;
                        year = (char)CWS_bcd_decode(raw[0]);
                        month = (char)CWS_bcd_decode(raw[1]);
                        day = (char)CWS_bcd_decode(raw[2]);
                        hour = (char)CWS_bcd_decode(raw[3]);
                        minute = (char)CWS_bcd_decode(raw[4]);
                        m = 5;
                        //n = sprintf(result, "%4d-%02d-%02d %02d:%02d", year + 2000, month, day, hour, minute);
                        String.Format(result, "%4d-%02d-%02d %02d:%02d", year + 2000, month, day, hour, minute);
                    }
                    break;
                case ws_types.tt:
                    m = 2;
                    //n = sprintf(result, "%02d:%02d", CWS_bcd_decode(raw[0]), CWS_bcd_decode(raw[1]));
                    String.Format(result, "%02d:%02d", CWS_bcd_decode(raw[0]), CWS_bcd_decode(raw[1]));
                    break;
                case ws_types.pb:
                    m = 1;
                    //n = sprintf(result, "%02x", raw[0]);
                    String.Format(result, "%02x", raw[0]);
                    break;
                case ws_types.wa:
                    m = 3;
                    // wind average - 12 bits split across a byte and a nibble
                    fresult = raw[0] + ((raw[2] & 0x0F) * 256);
                    fresult = fresult * scale + offset;
                    //n = sprintf(result, "%.*f", b, fresult);
                    String.Format(result, "%.*f", b, fresult);
                    break;
                case ws_types.wg:
                    m = 3;
                    // wind gust - 12 bits split across a byte and a nibble
                    fresult = raw[0] + ((raw[1] & 0xF0) * 16);
                    fresult = fresult * scale + offset;
                    //n = sprintf(result, "%.*f", b, fresult);
                    String.Format(result, "%.*f", b, fresult);
                    break;
                case ws_types.dp:
                    m = 1; //error checking for delay
                           // Scale outside temperature and calculate dew point
                    fresult = (float)CWS_dew_point(raw, scale, offset);
                    //n = sprintf(result, "%.*f", b, fresult);
                    String.Format(result, "%.*f", b, fresult);
                    break;
                default:
                    //MsgPrintf(0, "CWS_decode: Unknown type %u\n", ws_type);
                    break;
            }
            for (i = 0; i < m; ++i)
            {
                if (raw[i] != 0xFF) return n;
            }
            if (Convert.ToBoolean(m))
            {

                //MsgPrintf(0, "CWS_decode: invalid value at 0x%04X\n", raw);

                //sprintf(result, "--.-");
                String.Format(result, "--.-");
                n = 0;
            }
            return n;
        }

        double CWS_dew_point(byte[] raw, double scale, double offset)
        {
            ArraySegment<byte> myArrSegMid = new ArraySegment<byte>(raw, WSstruct.WS_TEMPERATURE_OUT, raw.Length - WSstruct.WS_TEMPERATURE_OUT);
            double temp = CWS_signed_short(myArrSegMid) * scale + offset;
            //double temp = CWS_signed_short(raw.Skip(WSstruct.WS_TEMPERATURE_OUT)) *scale + offset;
            double hum = raw[WSstruct.WS_HUMIDITY_OUT];

            // Compute dew point, using formula from
            // http://en.wikipedia.org/wiki/Dew_point.
            double a = 17.27;
            double b = 237.7;

            double gamma = ((a * temp) / (b + temp)) + Math.Log(hum / 100F);

            return (b * gamma) / (a - gamma);
        }

        /*---------------------------------------------------------------------------*/
        byte CWS_bcd_decode(byte byteIn)
        {
            byte lo = (byte)(byteIn & 0x0F);
            byte hi = (byte)(byteIn / 16);
            return (byte)(lo + (hi * 10));
        }

        /*---------------------------------------------------------------------------*/
        ushort CWS_unsigned_short(byte[] raw)
        {
            return (ushort)((raw[1] << 8) | raw[0]);
        }

        short CWS_signed_short(ArraySegment<byte> raw)
        {
            ushort us = (ushort)(((((ushort)raw.Array[1]) &0x7F) << 8) | raw.Array[0]);

            if (Convert.ToBoolean(raw.Array[1] & 0x80))  // Test for sign bit
                return (short)(-us); // Negative value
            else
                return (short)(us);  // Positive value
        }
    }
}
