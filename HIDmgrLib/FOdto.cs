namespace FineOffsetLib.WSdataStructs
{
    public class FOsettings {
        // The weather station stores signed shorts in a non-standard way.
        // Instead of two's compliment, sign-magnitude is used (8-bit is sign).
        // http://en.wikipedia.org/wiki/Signed_number_representations
        //#define MSB(v) 				(((v) >> 8) & 0xff)		// Most significant byte of short.
        //#define SIGN_BIT(v) 		(MSB(v) >> 7)			// Least Significant bit is sign bit.
        //#define MAGNITUDE_BITS(v) 	((v) & 0x7fff)
        //#define FIX_SIGN(v) 		((SIGN_BIT(v) ? -1 : 1) * MAGNITUDE_BITS(v))

        //
        // Based on http://www.jim-easterbrook.me.uk/weather/mm/
        //
        //typedef struct weather_settings_s {
        public byte[] magic_number = new byte[2];
        public byte read_period;      // Minutes between each stored reading.

        public byte unit_settings1;       // bit 0: indoor temperature: 0 = °C, 1 = °F
                                          // bit 1: outdoor temperature: 0 = °C, 1 = °F
                                          // bit 2: rain: 0 = mm, 1 = inch
                                          // bit 5: pressure: 1 = hPa
                                          // bit 6: pressure: 1 = inHg
                                          // bit 7: pressure: 1 = mmHg

        public byte unit_settings2;       // bit 0: wind speed: 1 = m/s
                                          // bit 1: wind speed: 1 = km/h
                                          // bit 2: wind speed: 1 = knot
                                          // bit 3: wind speed: 1 = m/h
                                          // bit 4: wind speed: 1 = bft

        public byte display_options1;     // bit 0: pressure: 0 = absolute, 1 = relative
                                          // bit 1: wind speed: 0 = average, 1 = gust
                                          // bit 2: time: 0 = 24 hour, 1 = 12 hour
                                          // bit 3: date: 0 = day-month-year, 1 = month-day-year
                                          // bit 4: time scale(?): 0 = 12 hour, 1 = 24 hour
                                          // bit 5: date: 1 = show year year
                                          // bit 6: date: 1 = show day name
                                          // bit 7: date: 1 = alarm time

        public byte display_options2;     // bit 0: outdoor temperature: 1 = temperature
                                          // bit 1: outdoor temperature: 1 = wind chill
                                          // bit 2: outdoor temperature: 1 = dew point
                                          // bit 3: rain: 1 = hour
                                          // bit 4: rain: 1 = day
                                          // bit 5: rain: 1 = week
                                          // bit 6: rain: 1 = month
                                          // bit 7: rain: 1 = total

        public byte alarm_enable1;        // bit 1: time
                                          // bit 2: wind direction
                                          // bit 4: indoor humidity low
                                          // bit 5: indoor humidity high
                                          // bit 6: outdoor humidity low
                                          // bit 7: outdoor humidity high

        public byte alarm_enable2;        // bit 0: wind average
                                          // bit 1: wind gust
                                          // bit 2: rain hourly
                                          // bit 3: rain daily
                                          // bit 4: absolute pressure low
                                          // bit 5: absolute pressure high
                                          // bit 6: relative pressure low
                                          // bit 7: relative pressure high

        public byte alarm_enable3;        // bit 0: indoor temperature low
                                          // bit 1: indoor temperature high
                                          // bit 2: outdoor temperature low
                                          // bit 3: outdoor temperature high
                                          // bit 4: wind chill low
                                          // bit 5: wind chill high
                                          // bit 6: dew point low
                                          // bit 7: dew point high

        public short timezone;            // Hours offset from Central European Time, so in the UK this should be set to -1.
                                          // In stations without a radio controlled clock this is always zero.

        public byte data_refreshed;        // Computer writes 0xAA to indicate a change of settings. Weather station clears value to acknowledge.
        public ushort data_count;          // Number of stored readings. Starts at zero, rises to 4080.

        public ushort current_pos;         // Address of the stored reading currently being created.
                                           // Starts at 256, rises to 65520 in steps of 16, then loops
                                           // back to 256. The data at this address is updated every 48 seconds or so,
                                           // until the read period is reached. Then the address is incremented and
                                           // the next record becomes current.
                                           // Substract 256 and divide by 16 to get the number of saved history entries.

        public ushort relative_pressure;                // Current relative (sea level) atmospheric pressure, multiply by 0.1 to get hPa.
        public ushort absolute_pressure;                // Current absolute atmospheric pressure, multiply by 0.1 to get hPa.
        public byte[] unknown = new byte[7];            // Usually all zero, but have also seen 0x4A7600F724030E. If you have something different, let me know!
        public byte[] datetime = new byte[5];           // Date-time values are stored as year (last two digits), month, day, hour and minute in binary coded decimal, two digits per byte.
        public byte alarm_inhumid_high;                 // alarm, indoor humidity, high.
        public byte alarm_inhumid_low;                  // alarm, indoor humidity, low.
        public short alarm_intemp_high = 0;             // alarm, indoor temperature, high. Multiply by 0.1 to get °C.
        public short alarm_intemp_low = 0;              // alarm, indoor temperature, low. Multiply by 0.1 to get °C.
        public short alarm_outhumid_high = 0;           // alarm, outdoor humidity, high.
        public short alarm_outhumid_low = 0;            // alarm, outdoor humidity, low
        public short alarm_outtemp_high = 0;            // alarm, outdoor temperature, high. Multiply by 0.1 to get °C.
        public short alarm_outtemp_low = 0;             // alarm, outdoor temperature, low. Multiply by 0.1 to get °C.
        public short alarm_windchill_high = 0;          // alarm, wind chill, high. Multiply by 0.1 to get °C.
        public short alarm_windchill_low = 0;           // alarm, wind chill, low. Multiply by 0.1 to get °C.
        public short alarm_dewpoint_high = 0;           // alarm, dew point, high. Multiply by 0.1 to get °C.
        public short alarm_dewpoint_low = 0;            // alarm, dew point, low. Multiply by 0.1 to get °C.
        public short alarm_abs_pressure_high = 0;       // alarm, absolute pressure, high. Multiply by 0.1 to get hPa.
        public short alarm_abs_pressure_low = 0;        // alarm, absolute pressure, low. Multiply by 0.1 to get hPa.
        public short alarm_rel_pressure_high = 0;       // alarm, relative pressure, high. Multiply by 0.1 to get hPa.
        public short alarm_rel_pressure_low = 0;        // alarm, relative pressure, low. Multiply by 0.1 to get hPa.
        public short alarm_avg_wspeed_beaufort = 0;     // alarm, average wind speed, Beaufort.
        public short alarm_avg_wspeed_ms = 0;           // alarm, average wind speed, m/s. Multiply by 0.1 to get m/s.
        public short alarm_gust_wspeed_beaufort = 0;    // alarm, gust wind speed, Beaufort.
        public short alarm_gust_wspeed_ms = 0;          // alarm, gust wind speed, m/s. Multiply by 0.1 to get m/s.
        public short alarm_wind_direction = 0;          // alarm, wind direction. Multiply by 22.5 to get ° from north.
        public ushort alarm_rain_hourly = 0;            // alarm, rain, hourly. Multiply by 0.3 to get mm.
        public ushort alarm_rain_daily = 0;             // alarm, rain, daily. Multiply by 0.3 to get mm.
        public ushort alarm_time = 0;                   // Hour & Time. BCD (http://en.wikipedia.org/wiki/Binary-coded_decimal)
        public short max_inhumid = 0;                   // maximum, indoor humidity, value.
        public short min_inhumid = 0;                   // minimum, indoor humidity, value.
        public short max_outhumid = 0;                  // maximum, outdoor humidity, value.
        public short min_outhumid = 0;                  // minimum, outdoor humidity, value.
        public short max_intemp = 0;                    // maximum, indoor temperature, value. Multiply by 0.1 to get °C.
        public short min_intemp = 0;                    // minimum, indoor temperature, value. Multiply by 0.1 to get °C.
        public short max_outtemp;                       // maximum, outdoor temperature, value. Multiply by 0.1 to get °C.
        public short min_outtemp;                       // minimum, outdoor temperature, value. Multiply by 0.1 to get °C.
        public short max_windchill;                     // maximum, wind chill, value. Multiply by 0.1 to get °C.
        public short min_windchill;                     // minimum, wind chill, value. Multiply by 0.1 to get °C.
        public short max_dewpoint;                      // maximum, dew point, value. Multiply by 0.1 to get °C.
        public short min_dewpoint;                      // minimum, dew point, value. Multiply by 0.1 to get °C.
        public ushort max_abs_pressure;                 // maximum, absolute pressure, value. Multiply by 0.1 to get hPa.
        public ushort min_abs_pressure;                 // minimum, absolute pressure, value. Multiply by 0.1 to get hPa.
        public ushort max_rel_pressure;                 // maximum, relative pressure, value. Multiply by 0.1 to get hPa.
        public ushort min_rel_pressure;                 // minimum, relative pressure, value. Multiply by 0.1 to get hPa.
        public ushort max_avg_wspeed;                   // maximum, average wind speed, value. Multiply by 0.1 to get m/s.
        public ushort max_gust_wspeed;                  // maximum, gust wind speed, value. Multiply by 0.1 to get m/s.
        public ushort max_rain_hourly;                  // maximum, rain hourly, value. Multiply by 0.3 to get mm.
        public ushort max_rain_daily;                   // maximum, rain daily, value. Multiply by 0.3 to get mm.
        public ushort max_rain_weekly;                  // maximum, rain weekly, value. Multiply by 0.3 to get mm.
        public ushort max_rain_monthly;                 // maximum, rain monthly, value. Multiply by 0.3 to get mm.
        public ushort max_rain_total;                   // maximum, rain total, value. Multiply by 0.3 to get mm.
        public byte[] max_inhumid_date = new byte[5];  // maximum, indoor humidity, when. Datetime in BCD-format.
        public byte[] min_inhumid_date = new byte[5];  // minimum, indoor humidity, when. Datetime in BCD-format.
        public byte[] max_outhumid_date = new byte[5]; // maximum, outdoor humidity, when. Datetime in BCD-format.
        public byte[] min_outhumid_date = new byte[5]; // minimum, outdoor humidity, when. Datetime in BCD-format.
        public byte[] max_intemp_date = new byte[5];   // maximum, indoor temperature, when. Datetime in BCD-format.
        public byte[] min_intemp_date = new byte[5];   // minimum, indoor temperature, when. Datetime in BCD-format.
        public byte[] max_outtemp_date = new byte[5];  // maximum, outdoor temperature, when. Datetime in BCD-format.
        public byte[] min_outtemp_date = new byte[5];  // minimum, outdoor temperature, when. Datetime in BCD-format.
        public byte[] max_windchill_date = new byte[5];// maximum, wind chill, when. Datetime in BCD-format.
        public byte[] min_windchill_date = new byte[5];// minimum, wind chill, when. Datetime in BCD-format.
        public byte[] max_dewpoint_date = new byte[5]; // maximum, dew point, when. Datetime in BCD-format.
        public byte[] min_dewpoint_date = new byte[5]; // minimum, dew point, when. Datetime in BCD-format.
        public byte[] max_abs_pressure_date = new byte[5]; // maximum, absolute pressure, when. Datetime in BCD-format.
        public byte[] min_abs_pressure_date = new byte[5]; // minimum, absolute pressure, when. Datetime in BCD-format.
        public byte[] max_rel_pressure_date = new byte[5]; // maximum, relative pressure, when. Datetime in BCD-format.
        public byte[] min_rel_pressure_date = new byte[5]; // minimum, relative pressure, when. Datetime in BCD-format.
        public byte[] max_avg_wspeed_date = new byte[5]; // maximum, average wind speed, when. Datetime in BCD-format.
        public byte[] max_gust_wspeed_date = new byte[5]; // maximum, gust wind speed, when. Datetime in BCD-format.
        public byte[] max_rain_hourly_date = new byte[5]; // maximum, rain hourly, when. Datetime in BCD-format.
        public byte[] max_rain_daily_date = new byte[5]; // maximum, rain daily, when. Datetime in BCD-format.
        public byte[] max_rain_weekly_date = new byte[5]; // maximum, rain weekly, when. Datetime in BCD-format.
        public byte[] max_rain_monthly_date = new byte[5]; // maximum, rain monthly, when. Datetime in BCD-format.
        public byte[] max_rain_total_date = new byte[5]; // maximum, rain total, when. Datetime in BCD-format.

        public string pressure_unit = "hPa";
        public string indoor_temperature_unit = "Celsius";
        //}
        //weather_settings_t;


    }

    public class FOweatherdata {
        public byte delay;             // Minutes since last stored reading.
        public byte in_humidity;       // Indoor humidity.
        public short in_temp;          // Indoor temperature. Multiply by 0.1 to get °C.
        public byte out_humidity;      // Outdoor humidity.
        public short out_temp;         // Outdoor temperature. Multiply by 0.1 to get °C.
        public ushort abs_pressure;    // Absolute pressure. Multiply by 0.1 to get hPa.
        public byte avg_wind_lowbyte;  // Average wind speed, low bits. Multiply by 0.1 to get m/s. (I've read elsewhere that the factor is 0.38. I don't know if this is correct.)
        public byte gust_wind_lowbyte; // Gust wind speed, low bits. Multiply by 0.1 to get m/s. (I've read elsewhere that the factor is 0.38. I don't know if this is correct.)
        public byte wind_highbyte;     // Wind speed, high bits. Lower 4 bits are the average wind speed high bits, upper 4 bits are the gust wind speed high bits.
        public byte wind_direction;    // Multiply by 22.5 to get ° from north. If bit 7 is 1, no valid wind direction.
        public ushort total_rain;      // Total rain. Multiply by 0.3 to get mm.
        public byte status;            // Bits.
                                // 7th bit (i.e. bit 6, 64) indicates loss of contact with sensors.
                                // 8th bit (i.e. bit 7, 128) indicates rain counter overflow.
        public byte[] raw_data = new byte[16];
    }

    public class FOweatheritem {
        private FOweatherdata _weatherdata;

        public int history_index;
        public long timestamp;  // in milliseconds
        public uint address;

        public FOweatheritem(FOweatherdata wdata) {
            _weatherdata = wdata;
        }

        public FOweatherdata WeatherData {
            get { return _weatherdata; }
            set { _weatherdata = value; }
        }
    }


    public struct bcd_date_t {
        public ushort year;
        public ushort month;
        public ushort day;
        public ushort hour;
        public ushort minute;
    }

    public struct tm {
        public int tm_sec;   // seconds after the minute - [0, 60] including leap second
        public int tm_min;   // minutes after the hour - [0, 59]
        public int tm_hour;  // hours since midnight - [0, 23]
        public int tm_mday;  // day of the month - [1, 31]
        public int tm_mon;   // months since January - [0, 11]
        public int tm_year;  // years since 1900
        public int tm_wday;  // days since Sunday - [0, 6]
        public int tm_yday;  // days since January 1 - [0, 365]
        public int tm_isdst; // daylight savings time flag
    }

    public enum TemprtUnits
    {
        Celsius , Fahrenheight
    };

    public enum PressureUnits
    {
        hPa, inHg, mmHg
    };
}
