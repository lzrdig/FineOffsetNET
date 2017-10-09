using System;
using System.Diagnostics;

using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.Info;

using FineOffsetLib.WSdataStructs;
using FineOffsetLib.Helpers;


namespace FineOffsetLib.HIDmgr
{
    public class MyUsbDevMgr
    {
        public const uint HISTORY_MAX = 4080;
        public const uint HISTORY_CHUNK_SIZE = 16;
        public const uint WEATHER_SETTINGS_CHUNK_SIZE = 256;
        public const uint HISTORY_START = WEATHER_SETTINGS_CHUNK_SIZE;
        public const uint HISTORY_END = (HISTORY_START + (HISTORY_MAX * HISTORY_CHUNK_SIZE));
        public const uint LOST_SENSOR_CONTACT_BIT = 6;
        public const uint RAIN_COUNTER_OVERFLOW_BIT = 7;

        private UsbDevice _usbDev;
        private FOsettings _devSettings;
        private FOweatheritem[] _history = new FOweatheritem[HISTORY_MAX];
        private bool _devInit;

        //private UsbInterfaceInfo _usbInterfaceInfo = null;
        //private UsbEndpointInfo _usbEndpointInfo = null;
        private UsbEndpointReader _reader = null;

        public MyUsbDevMgr(IUsbDevice usbDev)
        {
            _usbDev = usbDev as UsbDevice;
        }
        public MyUsbDevMgr(IUsbDevice usbDev, FOsettings devSettings)
        {
            _usbDev = usbDev as UsbDevice;
            _devSettings = devSettings;

            int i = 0;
            for (i = 0; i < HISTORY_MAX; i++)
            {
                _history[i] = new FOweatheritem(new FOweatherdata());
            }

        }

        public bool IsInit {
            get { return _devInit; }
            set { _devInit = value; }
        }
        public FOsettings WSettings {
            get { return (GetSettings()); }
            set { _devSettings = value; }
        }
        public FOweatheritem[] History {
            get { return _history; }
            set {
                if (value.Length != HISTORY_MAX)
                    throw new ArgumentOutOfRangeException(
                   $"The size of {nameof(value)} must be equal to {HISTORY_MAX}.");
                _history = value;
            }
        }


        public UsbEndpointReader MyDevReader {
            get { return _reader; }
            set { _reader = value; }
        }

        public void CloseDevHndl()
        {
            _usbDev = null;
        }

        public ErrorCode InitDevInterface()
        {
            ErrorCode ec = ErrorCode.None;
            bool result = false;

            _devInit = false;

            IUsbDevice wholeUsbDevice = _usbDev as IUsbDevice;
            try
            {
                // Select config #1
                result = wholeUsbDevice.SetConfiguration(1);

                // Claim interface #0.
                result = wholeUsbDevice.ClaimInterface(0);

                // Set Alternate interface #0.
                result = wholeUsbDevice.SetAltInterface(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }

            if (!result)
            {
                ec = ErrorCode.UnknownError;
            }
            else
            {
                _devInit = true;
            }
            return ec;
        }

        #region GetFunctions

        public FOsettings GetSettings()
        {
            bool bValidDev = ReferenceEquals(_usbDev, null);

            if (!bValidDev)
            {
                byte[] readBuffer = new byte[256];

                GetSettingsBlockRaw(ref readBuffer);


                _devSettings.magic_number[0] = readBuffer[0];
                _devSettings.magic_number[1] = readBuffer[1];
                _devSettings.read_period = readBuffer[16];
                _devSettings.unit_settings1 = readBuffer[17];
                _devSettings.unit_settings2 = readBuffer[18];
                _devSettings.display_options1 = readBuffer[19];
                _devSettings.display_options2 = readBuffer[20];
                _devSettings.alarm_enable1 = readBuffer[21];
                _devSettings.alarm_enable2 = readBuffer[22];
                _devSettings.alarm_enable3 = readBuffer[23];
                _devSettings.timezone = Convert.ToSByte((readBuffer[24] & 0x7f));
                _devSettings.data_refreshed = readBuffer[26];
                _devSettings.data_count = Convert.ToUInt16((readBuffer[27] & 0xff) | (readBuffer[28] << 8));
                _devSettings.current_pos = Convert.ToUInt16((readBuffer[30] & 0xff) | (readBuffer[31] << 8));
                _devSettings.relative_pressure = Convert.ToUInt16((readBuffer[32] & 0xff) | (readBuffer[33] << 8));
                _devSettings.absolute_pressure = Convert.ToUInt16((readBuffer[34] & 0xff) | (readBuffer[35] << 8));
                //memcpy(&_devSettings.unknown, &readBuffer[36], 7);
                Array.Copy(readBuffer, 36, _devSettings.unknown, 0, 7);
                //memcpy(&_devSettings.datetime, &readBuffer[43], 5);
                Array.Copy(readBuffer, 43, _devSettings.datetime, 0, 5);
                _devSettings.alarm_inhumid_high = readBuffer[48];
                _devSettings.alarm_inhumid_low = readBuffer[49];
                _devSettings.alarm_intemp_high = Helpers.MyExtensions.FIX_SIGN((readBuffer[50] & 0xff) | (readBuffer[51] << 8));
                _devSettings.alarm_intemp_low = Helpers.MyExtensions.FIX_SIGN((readBuffer[52] & 0xff) | (readBuffer[53] << 8));
                _devSettings.alarm_outhumid_high = Convert.ToInt16(readBuffer[54]);
                _devSettings.alarm_outhumid_low = Convert.ToInt16(readBuffer[55]);
                _devSettings.alarm_outtemp_high = Helpers.MyExtensions.FIX_SIGN((readBuffer[56] & 0xff) | (readBuffer[57] << 8));
                _devSettings.alarm_outtemp_low = Helpers.MyExtensions.FIX_SIGN((readBuffer[58] & 0xff) | (readBuffer[59] << 8));
                _devSettings.alarm_windchill_high = Helpers.MyExtensions.FIX_SIGN((readBuffer[60] & 0xff) | (readBuffer[61] << 8));
                _devSettings.alarm_windchill_low = Helpers.MyExtensions.FIX_SIGN((readBuffer[62] & 0xff) | (readBuffer[63] << 8));
                _devSettings.alarm_dewpoint_high = Helpers.MyExtensions.FIX_SIGN((readBuffer[64] & 0xff) | (readBuffer[65] << 8));
                _devSettings.alarm_dewpoint_low = Helpers.MyExtensions.FIX_SIGN((readBuffer[66] & 0xff) | (readBuffer[67] << 8));
                _devSettings.alarm_abs_pressure_high = Convert.ToInt16((readBuffer[68] & 0xff) | (readBuffer[69] << 8));
                _devSettings.alarm_abs_pressure_low = Convert.ToInt16((readBuffer[70] & 0xff) | (readBuffer[71] << 8));
                _devSettings.alarm_rel_pressure_high = Convert.ToInt16((readBuffer[72] & 0xff) | (readBuffer[73] << 8));
                _devSettings.alarm_rel_pressure_low = Convert.ToInt16((readBuffer[74] & 0xff) | (readBuffer[75] << 8));
                _devSettings.alarm_avg_wspeed_beaufort = Convert.ToInt16(readBuffer[76]);
                _devSettings.alarm_avg_wspeed_ms = Convert.ToInt16(readBuffer[77]);
                _devSettings.alarm_gust_wspeed_beaufort = Convert.ToInt16(readBuffer[79]);
                _devSettings.alarm_gust_wspeed_ms = Convert.ToInt16(readBuffer[80]);
                _devSettings.alarm_wind_direction = Convert.ToInt16(readBuffer[82]);
                _devSettings.alarm_rain_hourly = Convert.ToUInt16((readBuffer[83] & 0xff) | (readBuffer[84] << 8));
                _devSettings.alarm_rain_daily = Convert.ToUInt16((readBuffer[85] & 0xff) | (readBuffer[86] << 8));
                _devSettings.alarm_time = Convert.ToUInt16((readBuffer[87] & 0xff) | (readBuffer[88] << 8));
                _devSettings.max_inhumid = Convert.ToInt16(readBuffer[98]);
                _devSettings.min_inhumid = Convert.ToInt16(readBuffer[99]);
                _devSettings.max_outhumid = Convert.ToInt16(readBuffer[100]);
                _devSettings.min_outhumid = Convert.ToInt16(readBuffer[101]);
                _devSettings.max_intemp = Helpers.MyExtensions.FIX_SIGN((readBuffer[102] & 0xff) | (readBuffer[103] << 8));
                _devSettings.min_intemp = Helpers.MyExtensions.FIX_SIGN((readBuffer[104] & 0xff) | (readBuffer[105] << 8));
                _devSettings.max_outtemp = Helpers.MyExtensions.FIX_SIGN((readBuffer[106] & 0xff) | (readBuffer[107] << 8));
                _devSettings.min_outtemp = Helpers.MyExtensions.FIX_SIGN((readBuffer[108] & 0xff) | (readBuffer[109] << 8));
                _devSettings.max_windchill = Helpers.MyExtensions.FIX_SIGN((readBuffer[110] & 0xff) | (readBuffer[111] << 8));
                _devSettings.min_windchill = Helpers.MyExtensions.FIX_SIGN((readBuffer[112] & 0xff) | (readBuffer[113] << 8));
                _devSettings.max_dewpoint = Helpers.MyExtensions.FIX_SIGN((readBuffer[114] & 0xff) | (readBuffer[115] << 8));
                _devSettings.min_dewpoint = Helpers.MyExtensions.FIX_SIGN((readBuffer[116] & 0xff) | (readBuffer[117] << 8));
                _devSettings.max_abs_pressure = Convert.ToUInt16((readBuffer[118] & 0xff) | (readBuffer[119] << 8));
                _devSettings.min_abs_pressure = Convert.ToUInt16((readBuffer[120] & 0xff) | (readBuffer[121] << 8));
                _devSettings.max_rel_pressure = Convert.ToUInt16((readBuffer[122] & 0xff) | (readBuffer[123] << 8));
                _devSettings.min_rel_pressure = Convert.ToUInt16((readBuffer[124] & 0xff) | (readBuffer[125] << 8));
                _devSettings.max_avg_wspeed = Convert.ToUInt16((readBuffer[126] & 0xff) | (readBuffer[127] << 8));
                _devSettings.max_gust_wspeed = Convert.ToUInt16((readBuffer[128] & 0xff) | (readBuffer[129] << 8));
                _devSettings.max_rain_hourly = Convert.ToUInt16((readBuffer[130] & 0xff) | (readBuffer[131] << 8));
                _devSettings.max_rain_daily = Convert.ToUInt16((readBuffer[132] & 0xff) | (readBuffer[133] << 8));
                _devSettings.max_rain_weekly = Convert.ToUInt16((readBuffer[134] & 0xff) | (readBuffer[135] << 8));
                _devSettings.max_rain_monthly = Convert.ToUInt16((readBuffer[136] & 0xff) | (readBuffer[137] << 8));
                _devSettings.max_rain_total = Convert.ToUInt16((readBuffer[138] & 0xff) | (readBuffer[139] << 8));
                //memcpy(&_devSettings.max_inhumid_date, &readBuffer[141], sizeof(_devSettings.max_inhumid_date));
                Array.Copy(readBuffer, 141, _devSettings.max_inhumid_date, 0, _devSettings.max_inhumid_date.Length);
                //memcpy(&_devSettings.min_inhumid_date, &readBuffer[146], sizeof(_devSettings.min_inhumid_date));
                Array.Copy(readBuffer, 146, _devSettings.min_inhumid_date, 0, _devSettings.min_inhumid_date.Length);
                //memcpy(&_devSettings.max_outhumid_date, &readBuffer[151], sizeof(_devSettings.max_outhumid_date));
                Array.Copy(readBuffer, 151, _devSettings.max_outhumid_date, 0, _devSettings.max_outhumid_date.Length);
                //memcpy(&_devSettings.min_outhumid_date, &readBuffer[156], sizeof(_devSettings.min_outhumid_date));
                Array.Copy(readBuffer, 156, _devSettings.min_outhumid_date, 0, _devSettings.min_outhumid_date.Length);
                //memcpy(&_devSettings.max_intemp_date, &readBuffer[161], sizeof(_devSettings.max_intemp_date));
                Array.Copy(readBuffer, 161, _devSettings.max_intemp_date, 0, _devSettings.max_intemp_date.Length);
                //memcpy(&_devSettings.min_intemp_date, &readBuffer[166], sizeof(_devSettings.min_intemp_date));
                Array.Copy(readBuffer, 166, _devSettings.min_intemp_date, 0, _devSettings.min_intemp_date.Length);
                //memcpy(&_devSettings.max_outtemp_date, &readBuffer[171], sizeof(_devSettings.max_outtemp_date));
                Array.Copy(readBuffer, 171, _devSettings.max_outtemp_date, 0, _devSettings.max_outtemp_date.Length);
                //memcpy(&_devSettings.min_outtemp_date, &readBuffer[176], sizeof(_devSettings.min_outtemp_date));
                Array.Copy(readBuffer, 176, _devSettings.min_outtemp_date, 0, _devSettings.min_outtemp_date.Length);
                //memcpy(&_devSettings.max_windchill_date, &readBuffer[181], sizeof(_devSettings.max_windchill_date));
                Array.Copy(readBuffer, 181, _devSettings.max_windchill_date, 0, _devSettings.max_windchill_date.Length);
                //memcpy(&_devSettings.min_windchill_date, &readBuffer[186], sizeof(_devSettings.min_windchill_date));
                Array.Copy(readBuffer, 186, _devSettings.min_windchill_date, 0, _devSettings.min_windchill_date.Length);
                //memcpy(&_devSettings.max_dewpoint_date, &readBuffer[191], sizeof(_devSettings.max_dewpoint_date));
                Array.Copy(readBuffer, 191, _devSettings.max_dewpoint_date, 0, _devSettings.max_dewpoint_date.Length);
                //memcpy(&_devSettings.min_dewpoint_date, &readBuffer[196], sizeof(_devSettings.min_dewpoint_date));
                Array.Copy(readBuffer, 196, _devSettings.min_dewpoint_date, 0, _devSettings.min_dewpoint_date.Length);
                //memcpy(&_devSettings.max_abs_pressure_date, &readBuffer[201], sizeof(_devSettings.max_abs_pressure_date));
                Array.Copy(readBuffer, 201, _devSettings.max_abs_pressure_date, 0, _devSettings.max_abs_pressure_date.Length);
                //memcpy(&_devSettings.min_abs_pressure_date, &readBuffer[206], sizeof(_devSettings.min_abs_pressure_date));
                Array.Copy(readBuffer, 206, _devSettings.min_abs_pressure_date, 0, _devSettings.min_abs_pressure_date.Length);
                //memcpy(&_devSettings.max_rel_pressure_date, &readBuffer[211], sizeof(_devSettings.max_rel_pressure_date));
                Array.Copy(readBuffer, 211, _devSettings.max_rel_pressure_date, 0, _devSettings.max_rel_pressure_date.Length);
                //memcpy(&_devSettings.min_rel_pressure_date, &readBuffer[216], sizeof(_devSettings.min_rel_pressure_date));
                Array.Copy(readBuffer, 216, _devSettings.min_rel_pressure_date, 0, _devSettings.min_rel_pressure_date.Length);
                //memcpy(&_devSettings.max_avg__devSettingspeed_date, &readBuffer[221], sizeof(_devSettings.max_avg__devSettingspeed_date));
                Array.Copy(readBuffer, 221, _devSettings.max_avg_wspeed_date, 0, _devSettings.max_avg_wspeed_date.Length);
                //memcpy(&_devSettings.max_gust__devSettingspeed_date, &readBuffer[226], sizeof(_devSettings.max_gust__devSettingspeed_date));
                Array.Copy(readBuffer, 226, _devSettings.max_gust_wspeed_date, 0, _devSettings.max_gust_wspeed_date.Length);
                //memcpy(&_devSettings.max_rain_hourly_date, &readBuffer[231], sizeof(_devSettings.max_rain_hourly_date));
                Array.Copy(readBuffer, 231, _devSettings.max_rain_hourly_date, 0, _devSettings.max_rain_hourly_date.Length);
                //memcpy(&_devSettings.max_rain_daily_date, &readBuffer[236], sizeof(_devSettings.max_rain_daily_date));
                Array.Copy(readBuffer, 236, _devSettings.max_rain_daily_date, 0, _devSettings.max_rain_daily_date.Length);
                //memcpy(&_devSettings.max_rain_weekly_date, &readBuffer[241], sizeof(_devSettings.max_rain_weekly_date));
                Array.Copy(readBuffer, 241, _devSettings.max_rain_weekly_date, 0, _devSettings.max_rain_weekly_date.Length);
                //memcpy(&_devSettings.max_rain_monthly_date, &readBuffer[246], sizeof(_devSettings.max_rain_monthly_date));
                Array.Copy(readBuffer, 246, _devSettings.max_rain_monthly_date, 0, _devSettings.max_rain_monthly_date.Length);
                //memcpy(&_devSettings.max_rain_total_date, &readBuffer[251], sizeof(_devSettings.max_rain_total_date));
                Array.Copy(readBuffer, 251, _devSettings.max_rain_total_date, 0, _devSettings.max_rain_total_date.Length);
            }

            return _devSettings;
        }
        private void GetSettingsBlockRaw(ref byte[] readBuffer)
        {
            bool bValidDev = ReferenceEquals(_usbDev, null);

            if (!bValidDev)
            {
                ErrorCode ec = ErrorCode.None;
                UsbInterfaceInfo usbInterfaceInfo = null;
                UsbEndpointInfo usbEndpointInfo = null;

                // Look and see if its endpoint matches UsbConstants.ENDPOINT_DIR_MASK
                UsbEndpointBase.LookupEndpointInfo(_usbDev.Configs[0], UsbConstants.ENDPOINT_DIR_MASK, out usbInterfaceInfo, out usbEndpointInfo);

                // open read endpoint 1.
                _reader = _usbDev.OpenEndpointReader(ReadEndpointID.Ep01, 32, EndpointType.Interrupt);

                ushort address = 0x00;
                string numBytesRecord = "";

                //Insure that the size of the buffer is 256 bytes. The data in the buffer will be overwritten in any case
                if (readBuffer.Length != 256)
                    Array.Resize(ref readBuffer, 256);

                //read first 256 bytes (= 0x100) where the settings are stored
                while (address < 0x100 && ec == ErrorCode.None)
                {
                    Tuple<ErrorCode, int> res = ReadFromAddr(ref _reader, usbInterfaceInfo, address, ref readBuffer);

                    if (res.Item1 != ErrorCode.Success) Debug.WriteLine("reading address = %d failed", address);
                    address += 0x20;

                    numBytesRecord += Convert.ToString(res.Item2) + ";";
                }
            }
        }

        public void GetWeatherData(int items_to_read)
        {
            // Read events in reverse.
            // Loop through the events in reverse order, starting with the last recorded one
            // and calculate the timestamp for each event. We only know the current
            // weather station date/time + the delay in minutes between each event, so
            // we can only get the timestamps by doing it this way.

            // Convert the weather station date from a BCD date to unix date.
            long station_date = bcd_to_unix_date(parse_bcd_date(_devSettings.datetime));

            uint total_seconds = 0;
            uint seconds = 0;
            uint history_begin = (_devSettings.current_pos + HISTORY_CHUNK_SIZE);
            int history_index;
            int j;
            int i = 0;
            uint history_address;

            //Debug.WriteLine("Start reading history blocks\n");
            //Debug.WriteLine("Index\tTimestamp\t\tDelay\n");

            for (history_address = _devSettings.current_pos, i = (Convert.ToInt32(HISTORY_MAX) - 1), j = 0;
                (j < items_to_read);
                history_address -= HISTORY_CHUNK_SIZE, i--, j++)
            {
                // The buffer is full so it acts as a circular buffer, so we need to
                // wrap to the end to get the next item.
                if (history_address < HISTORY_START)
                {
                    history_address = HISTORY_END - (HISTORY_START - history_address);
                }

                // Calculate the index we're at in the history, from 0-4080.
                if (_devSettings.data_count < HISTORY_MAX)
                {
                    history_index = 1 + Convert.ToUInt16((history_address - HISTORY_START) / HISTORY_CHUNK_SIZE); // Normal.
                }
                else
                {
                    history_index = 1 + Convert.ToUInt16(((history_address - HISTORY_START) + (HISTORY_END - history_begin)) / HISTORY_CHUNK_SIZE); // Circular buffer.
                }

                // Read history chunk.
                _history[i].history_index = history_index;
                _history[i].address = history_address;

                //history[i].data = get_history_chunk(h, ref _devSettings, history_address);
                _history[i].WeatherData = get_history_chunk(_devSettings, Convert.ToUInt16(history_address));

                // Calculate timestamp.
                _history[i].timestamp = (long)(station_date - total_seconds);
                //seconds = history[i].data.delay * 60;
                seconds = Convert.ToUInt32(Convert.ToInt32(Convert.ToByte(_history[i].WeatherData.delay)) * 60);
                total_seconds += seconds;

                // Debug print.	
                Debug.WriteLine("DEBUG: Seconds before current event = {0}", total_seconds);
                Debug.WriteLine("DEBUG: Temp = {0} C", _history[i].WeatherData.in_temp * 0.1f);
                Debug.WriteLine("DEBUG: {0},\t{1},\t{2:d} minutes",
                    i,
                    _history[i].timestamp, //get_timestamp(history[i].timestamp),
                    Convert.ToInt32(_history[i].WeatherData.delay));
            }
        }

        private FOweatherdata get_history_chunk(FOsettings ws, ushort history_pos)
        {
            byte[] buf = new byte[history_pos + 2 * HISTORY_CHUNK_SIZE]; //libsub reads data into the history_pos location of the buffer
            FOweatherdata wd = new FOweatherdata();
            UsbInterfaceInfo usbInterfaceInfo = null;
            UsbEndpointInfo usbEndpointInfo = null;

            //wd.status = '\0';

            bool bValidDev = ReferenceEquals(_usbDev, null);
            if (!bValidDev)
            {
                // Look and see if its endpoint matches UsbConstants.ENDPOINT_DIR_MASK
                UsbEndpointBase.LookupEndpointInfo(_usbDev.Configs[0], UsbConstants.ENDPOINT_DIR_MASK, out usbInterfaceInfo, out usbEndpointInfo);

                //open read endpoint 1.
                using (_reader = _usbDev.OpenEndpointReader(ReadEndpointID.Ep01, 32, EndpointType.Interrupt))
                {
                    //read_weather_address(h, history_pos, buf)
                    Tuple<ErrorCode, int> res = ReadFromAddr(ref _reader, usbInterfaceInfo, history_pos, ref buf);
                }

                wd.delay = buf[0 + history_pos];
                wd.in_humidity = buf[1 + history_pos];
                wd.in_temp = Helpers.MyExtensions.FIX_SIGN((buf[2 + history_pos] & 0xff) | (buf[3 + history_pos] << 8));
                wd.out_humidity = buf[4 + history_pos];
                wd.out_temp = Helpers.MyExtensions.FIX_SIGN((buf[5 + history_pos] & 0xff) | (buf[6 + history_pos] << 8));
                wd.abs_pressure = Convert.ToUInt16((buf[7 + history_pos] & 0xff) | (buf[8 + history_pos] << 8));
                wd.avg_wind_lowbyte = buf[9 + history_pos];
                wd.gust_wind_lowbyte = buf[10 + history_pos];
                wd.wind_highbyte = buf[11 + history_pos];
                wd.wind_direction = buf[12 + history_pos];
                wd.total_rain = Convert.ToUInt16((buf[13 + history_pos] & 0xff) | (buf[14 + history_pos] << 8));
                wd.status = buf[15 + history_pos];

                //memcpy(&d.raw_data, b, sizeof(d.raw_data));
                for (int i = 0; i < wd.raw_data.Length; i++)
                    wd.raw_data[i] = buf[i + history_pos];
            }

            return wd;
        }

        #endregion

        #region Set Functions
        //
        // Sets a single byte at a specified offset in the fixed weather settings chunk.
        //
        private int set_weather_setting_byte(ref UsbDevice devh, int offset, byte data)
        {
            Debug.Assert(offset < WEATHER_SETTINGS_CHUNK_SIZE);
            return write_weather_1(ref devh, Convert.ToUInt16(offset), data);
        }

        //
        // Writes a notify byte so the weather station knows a setting has changed.
        //
        int notify_weather_setting_change(ref UsbDevice devh)
        {
            // Write 0xAA to address 0x1a to indicate a change of settings.
            return set_weather_setting_byte(ref devh, 0x1a, 0xaa);
        }

        //
        // Sets a weather setting at a given offset in the weather settings chunk.
        //
        int set_weather_setting(ref UsbDevice devh, int offset, byte[] data, int len)
        {
            int status = 0; //success
            for (int i = 0; i < len; i++)
            {
                if (set_weather_setting_byte(ref devh, offset, data[i]) != 0)
                {
                    return -1;
                }
            }

            status = notify_weather_setting_change(ref devh);

            return status;
        }

        //// TODO: Remake this to weather_settings_t structure and write all changes in that to the device.
        //int set_weather_settings_bulk(struct usb_dev_handle * h, unsigned int change_offset, char* data, unsigned int len)
        //{
        //	unsigned offset;
        ////unsigned int i;
        //char buf[WEATHER_SETTINGS_CHUNK_SIZE];

        //    // Make sure we're not trying to write outside the settings buffer.
        //    assert((change_offset + len) < WEATHER_SETTINGS_CHUNK_SIZE);


        //    get_settings_block_raw(h, buf, sizeof(buf));

        //    // Change the settings.
        //    memcpy(&buf[change_offset], data, len);

        //	// Send back the settings in 3 32-bit chunks.
        //	for (offset = 0; offset< (32 * 3); offset += 32)
        //	{

        //        write_weather_32(h, offset, &buf[offset]);

        //		if (read_weather_ack(h) != 0)
        //		{
        //			return -1;
        //		}
        //	}


        //    notify_weather_setting_change(h);

        //	return 0;
        //}

        int set_timezone(ref UsbDevice devh, sbyte timezone)
        {
            byte[] tmz = new byte[1];
            tmz[0] = Convert.ToByte(timezone);

            return set_weather_setting(ref devh, 24, tmz, 1);
        }

        int set_delay(ref UsbDevice devh, sbyte delay)
        {
            byte[] del = new byte[1];
            del[0] = Convert.ToByte(delay);

            return set_weather_setting(ref devh, 16, del, 1);
        }
        #endregion

        #region Helpers for Write
        //
        //Reads weather ack message when writing setting data.    
        //
        int read_weather_ack(ref UsbDevice devh, int addr)
        {
            int i;
            byte[] buf = new byte[8];

            read_weather_msg(ref devh, buf, addr);

            // The ack should consist of just 0xa5.
            for (i = 0; i < 8; i++)
            {
                //debug_printf(2, "%x ", (buf[i] & 0xff));

                if ((buf[i] & 0xff) != 0xa5)
                    return -1;
            }

            //    debug_printf(2, "\n");

            return 0;
        }

        //
        // Writes 1 byte of data to the weather station.
        //
        int write_weather_1(ref UsbDevice h, ushort addr, byte data)
        {
            byte[] msg = new byte[] { 0xa2, Convert.ToByte((addr) >> 8), Convert.ToByte(addr & 0xff), 0x20, 0xa2, data, 0, 0x20 };

            send_usb_msgbuf(ref h, msg, 8);

            return read_weather_ack(ref h, addr);
        }

        //
        // Writes 32 bytes of data to the weather station.
        //
        int write_weather_32(ref UsbDevice h, ushort addr, byte[] data)
        {

            byte[] msg = new byte[] { 0xa0, Convert.ToByte(addr >> 8), Convert.ToByte(addr & 0xff), 0x20, 0xa0, 0, 0, 0x20 };


            send_usb_msgbuf(ref h, msg, 8);     // Send write command.

            send_usb_msgbuf(ref h, data, 32);   // Send data.

            return read_weather_ack(ref h, addr);
        }

        #endregion

        //
        // All data from the weather station is read in 32 byte chunks.
        //
        int read_weather_msg(ref UsbDevice devh, byte[] readBuffer, int address)
        {
            int numBytes = 0;

            //ret = usb_interrupt_read(devh, ENDPOINT_INTERRUPT_ADDRESS, buf,   0x20, USB_TIMEOUT);
            ErrorCode ec = _reader.Read(readBuffer, address, 0x20, 500, out numBytes);

            if (ec == ErrorCode.Win32Error)
                ec = _reader.Read(readBuffer, address, 0x20, 500, out numBytes);

            return numBytes;
        }



        //
        // Sends a USB message to the device from a given buffer.
        //
        int send_usb_msgbuf(ref UsbDevice h, byte[] msg, int msgsize)
        {
            int bytes_written = 0;
            UsbSetupPacket usbPacket = new UsbSetupPacket();
            UsbInterfaceInfo usbInterfaceInfo = null;
            UsbEndpointInfo usbEndpointInfo = null;

            Debug.WriteLine(2, "--> ");

            //print_bytes(2, msg, msgsize);

            //The code from below is equivalent to the following line from libusb C code
            //bytes_written = usb_control_msg(h, USB_TYPE_CLASS + USB_RECIP_INTERFACE, 9, 0x200, 0, msg, msgsize, USB_TIMEOUT);

            // Look and see if its endpoint matches UsbConstants.ENDPOINT_DIR_MASK
            UsbEndpointBase.LookupEndpointInfo(_usbDev.Configs[0], UsbConstants.ENDPOINT_DIR_MASK, out usbInterfaceInfo, out usbEndpointInfo);

            //                                   request type                       request                             value    index           data attached to request      size,    timeout
            //int ret = usb_control_msg(devh, USB_TYPE_CLASS + USB_RECIP_INTERFACE,                                       9   ,   0x200,                0  ,         msg,                   8      , 1000);
            usbPacket = new UsbSetupPacket((byte)UsbCtrlFlags.RequestType_Class + (byte)UsbCtrlFlags.Recipient_Interface, 9, (short)0x200, usbInterfaceInfo.Descriptor.InterfaceID, Convert.ToInt16(msgsize));

            var ret = _usbDev.ControlTransfer(ref usbPacket, msg, msg.Length, out bytes_written);

            Debug.Assert(bytes_written == msgsize);

            return bytes_written;
        }


        /// <summary>
        /// High-Level USB read function. Returns data from the address of device buffer into the client <paramref name="readBuffer"/>.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="usbInterfaceInfo"></param>
        /// <param name="address"></param>
        /// <param name="readBuffer"></param>
        /// <returns></returns>
        private Tuple<ErrorCode, int> ReadFromAddr(ref UsbEndpointReader reader, UsbInterfaceInfo usbInterfaceInfo, ushort address, ref byte[] readBuffer)
        {
            ErrorCode ec = ErrorCode.None;
            bool ret = false;
            int numBytes = 0;
            string numBytesRecord = "";
            int lengthTransferred = 0;
            UsbSetupPacket usbPacket = new UsbSetupPacket();

            try
            {
                byte lowbyte = (byte)(address & 0xFF);
                byte highbyte = (byte)(address >> 8);

                //                                   request type                       request                             value    index           data attached to request      size,    timeout
                //int ret = usb_control_msg(devh, USB_TYPE_CLASS + USB_RECIP_INTERFACE,                                       9   ,   0x200,                0  ,         tbuf,                   8      , 1000);
                usbPacket = new UsbSetupPacket((byte)UsbCtrlFlags.RequestType_Class + (byte)UsbCtrlFlags.Recipient_Interface, 9, (short)0x200, usbInterfaceInfo.Descriptor.InterfaceID, 8);

                byte[] cmd = new byte[] { 0xA1, highbyte, lowbyte, 0x20, 0xA1, 0x00, 0x00, 0x20 };

                ret = _usbDev.ControlTransfer(ref usbPacket, cmd, cmd.Length, out lengthTransferred);

                if (ret)
                {
                    //                              endpoint, buffer , size, timeout
                    //ret = usb_interrupt_read(devh, 0x81,      buf,   0x20, 1000);
                    ec = reader.Read(readBuffer, address, 0x20, 500, out numBytes);

                    if (ec == ErrorCode.Win32Error)
                        ec = reader.Read(readBuffer, address, 0x20, 500, out numBytes);

                    numBytesRecord += Convert.ToString(numBytes) + ";";
                }
            }
            catch (ArgumentOutOfRangeException bufEx)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + bufEx.Message);
            }

            return new Tuple<ErrorCode, int>(ec, numBytes);
        }


        //
        // Parses a date in BCD format (http://en.wikipedia.org/wiki/Binary-coded_decimal).
        // Each nibble of each byte corresponds to one number. The first byte contains the year.
        //
        private bcd_date_t parse_bcd_date(byte[] date)
        {
            bcd_date_t d;

            d.year = Convert.ToUInt16(2000 + (((date[0] >> 4) & 0xf) * 10) + (date[0] & 0xf));
            d.month = Convert.ToUInt16((((date[1] >> 4) & 0xf) * 10) + (date[1] & 0xf));
            d.day = Convert.ToUInt16((((date[2] >> 4) & 0xf) * 10) + (date[2] & 0xf));
            d.hour = Convert.ToUInt16((((date[3] >> 4) & 0xf) * 10) + (date[3] & 0xf));
            d.minute = Convert.ToUInt16((((date[4] >> 4) & 0xf) * 10) + (date[4] & 0xf));

            return d;
        }

        private long bcd_to_unix_date(bcd_date_t date)
        {
            //return Millisecons corresponding to the time on the station
            long rawtime = 0;

            //tm timeinfo = new tm();

            //timeinfo.tm_year = date.year - 1900;
            //timeinfo.tm_mon = date.month - 1;
            //timeinfo.tm_mday = date.day;
            //timeinfo.tm_hour = date.hour;
            //timeinfo.tm_min = date.minute;
            //timeinfo.tm_isdst = 1;

            DateTime timeinfo = new DateTime(date.year, date.month, date.day, date.hour, date.minute, 0);


            //rawtime = mktime(&timeinfo);

            rawtime = Convert.ToInt64(timeinfo.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);

            return rawtime;
        }






        public void print_summary(FOsettings ws, FOweatheritem item)
        {
            FOweatherdata wd = item.WeatherData;

            bool contact = has_contact_with_sensor(item.WeatherData);

            Console.WriteLine("Use --help for more options.\n\n");

            Console.WriteLine("Indoor:");
            Console.WriteLine("  Temperature:\t\t{0} C", wd.in_temp * 0.1f);
            Console.WriteLine("  Humidity:\t\t{0:d} %", Convert.ToInt32(Convert.ToByte(wd.in_humidity)));
            Console.WriteLine("");
            Console.WriteLine("Outdoor: {0}", (!contact) ? "NO CONTACT WITH SENSOR" : "");

            // Only show current data if we have sensor contact.
            if (contact)
            {
                Console.WriteLine("  Temperature:\t\t%0.1f C\n", wd.out_temp * 0.1f);
                //Console.WriteLine("  Wind chill:\t\t%0.1f C\n", calculate_windchill(wd));
                //Console.WriteLine("  Dewpoint:\t\t%0.1f C\n", calculate_dewpoint(wd));
                Console.WriteLine("  Humidity:\t\t%u%%\n", wd.out_humidity);
                Console.WriteLine("  Absolute pressure:\t%0.1f hPa\n", wd.abs_pressure * 0.1f);
                //Console.WriteLine("  Relative pressure:\t%0.1f hPa\n", calculate_rel_pressure(wd));
                //Console.WriteLine("  Average windspeed:\t%0.1f m/s\n", convert_avg_windspeed(wd));
                //Console.WriteLine("  Gust wind speed:\t%2.1f m/s\n", convert_gust_windspeed(wd));
                //Console.WriteLine("  Wind direction:\t%0.0f %s\n", wd.wind_direction * 22.5f, get_wind_direction(wd.wind_direction));
                Console.WriteLine("  Total rain:\t\t%0.1f mm\n", wd.total_rain * 0.3f);
            }

            Console.WriteLine("\n");
        }

        //   1, 2010-09-13 13:41:34, 2010-08-13 14:46:53,  30,   53,  26.1,   55,  25.2,  15.5,  24.1,  1019.3,  1013.3,  3.1,   2,  5.8,   4,  10,  SW, 		   34,    10.2,     0.0,     0.0,     0.0,     0.0,     0.0,      0.0, 0, 0, 0, 0, 0, 0, 0, 0, 000100, 1E 35 05 01 37 FC 00 D1 27 1F 3A 00 0A 22 00 00 ,
        public void print_history_item(FOweatheritem item)
        {
            FOweatherdata wd = item.WeatherData;
            int i;

            //                  1      2    3     4      5     6    7    8    9   10	11	  12    13    14    15    16    17    18    19    20    21    22   23     24   25    26    27    28    29    30    31    32    33  34  35
            Console.WriteLine("{0:d}, {1}, {2}, {3:d}, {4:d}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {32}, {34}, {34}, {34}, ",
                item.history_index,                                 // 1  Index.
                DateTime.Now.ToLocalTime().ToString(),              // 2  Date/time of "read operation" from weather station.
                get_timestamp(item.timestamp),                      // 3  Date/time data was recored.
                Convert.ToByte(Convert.ToInt32(wd.delay)),          // 4  Minutes since previous reading.
                Convert.ToByte(Convert.ToInt32(wd.in_humidity)),    // 5  Indoor humidity.
                wd.in_temp * 0.1f,                                  // 6  Indoor temperature.
                Convert.ToByte(Convert.ToInt32(wd.out_humidity)),   // 7  Outdoor humidity.
                wd.out_temp * 0.1f,                                 // 8  Outdoor temperature.
                "NA",//calculate_dewpoint(wd),           // 9  Dew point.
                "NA",//calculate_windchill(wd),          // 10 Wind chill.
                wd.abs_pressure * 0.1f,                        // 11 Absolute pressure.
                wd.abs_pressure * 0.1f,                        // 12 Relative pressure. // TODO: Calculate this somehow!!!
                "NA",//convert_avg_windspeed(wd),                      // 13 Wind average (m/s).
                "NA",//calculate_beaufort(convert_avg_windspeed(wd)),  // 14 Wind average Beaufort. // TODO: Calculate this, integer.
                "NA",//convert_gust_windspeed(wd),                     // 15 Wind gust (m/s).
                "NA",//calculate_beaufort(convert_gust_windspeed(wd)), // 16 Wind gust (Beaufort). // TODO: Calculate this, integer.
                wd.wind_direction * 22.5f,                     // 17 Wind direction.
                "NA",//get_wind_direction(wd.wind_direction),         // 18 Wind direction, text.
                wd.total_rain,                                 // 19 Rain ticks integer. Cumulative count of number of times rain gauge has tipped. Resets to zero if station's batteries removed
                wd.total_rain * 0.3f,                          // 20 mm rain total. Column 19 * 0.3, but does not reset to zero, stays fixed until ticks catch up.
                0.0,                                            // 21 Rain since last reading. mm
                0.0,                                            // 22 Rain in last hour. mm
                0.0,                                            // 23 Rain in last 24 hours. mm
                0.0,                                            // 24 Rain in last 7 days. mm
                0.0,                                            // 25 Rain in last 30 days. mm
                0.0,                                            // 26 Rain total in last year? mm. This is the same as column 20 in my data
                ((wd.status >> 0) & 0x1),                      // 27 Status bit 0.
                ((wd.status >> 1) & 0x1),                      // 28 Status bit 1.
                ((wd.status >> 2) & 0x1),                      // 29 Status bit 2.
                ((wd.status >> 3) & 0x1),                      // 30 Status bit 3.
                ((wd.status >> 4) & 0x1),                      // 31 Status bit 4.
                ((wd.status >> 5) & 0x1),                      // 32 Status bit 5.
                ((wd.status >> 6) & 0x1),                      // 33 Status bit 6.
                ((wd.status >> 7) & 0x1),                      // 34 Status bit 7.
                item.address);                                 // 35 Data address.

            for (i = 0; i < 16; i++)
            {
                Console.WriteLine("{0} ", wd.raw_data[i]);
            }

            Console.WriteLine(",\n");
        }

        public void print_status(FOsettings ws)
        {
            Console.WriteLine("Magic number:\t\t\t0x{0}{1}", ws.magic_number[0], ws.magic_number[1] & 0xff);
            Console.WriteLine("Read period:\t\t\t{0} minutes", ws.read_period);
            Console.WriteLine("Timezone:\t\t\tCET{0}{1}", (ws.timezone > 0) ? "+" : "-", ws.timezone);
            Console.WriteLine("Data count:\t\t\t{0} out of {1} ({2}%)", ws.data_count, HISTORY_MAX, (float)ws.data_count / HISTORY_MAX * 100);
            Console.WriteLine("Current memory position:\t{0} (0x{1})", ws.current_pos, ws.current_pos);
            Console.WriteLine("Current relative pressure:\t{0} hPa", ws.relative_pressure * 0.1f);
            Console.WriteLine("Current Absolute pressure:\t{0} hPa", ws.absolute_pressure * 0.1f);
            Console.Write("Unknown bytes:\t\t\t0x");
            {
                int i;
                for (i = 0; i < 7; i++) Console.Write("{0}", ws.unknown[i]);
            }
            Console.WriteLine("");
            Console.Write("Station date/time:\t\t");
            print_bcd_date(ws.datetime);
            Console.WriteLine("");
        }

        public void print_settings(FOsettings ws)
        {
            Console.WriteLine("Unit settings:");
            Console.WriteLine("  Indoor temperature unit:\t{0}", (Convert.ToBoolean(ws.unit_settings1 & (1 << 0))) ? "Fahrenheit" : "Celcius");
            Console.WriteLine("  Outdoor temperature unit:\t{0}", (Convert.ToBoolean(ws.unit_settings1 & (1 << 1))) ? "Fahrenheit" : "Celcius");
            Console.WriteLine("  Rain unit:\t\t\t{0}", (Convert.ToBoolean(ws.unit_settings1 & (1 << 2))) ? "mm" : "inch");

            Console.Write("  Pressure unit:\t\t");
            if (Convert.ToBoolean(ws.unit_settings1 & (1 << 5)))
                Console.Write("hPa");
            else if (Convert.ToBoolean(ws.unit_settings1 & (1 << 6)))
                Console.Write("inHg");
            else if (Convert.ToBoolean(ws.unit_settings1 & (1 << 7)))
                Console.Write("mmHg");
            Console.WriteLine("");

            Console.Write("  Wind speed unit:\t\t");
            if (Convert.ToBoolean(ws.unit_settings2 & (1 << 0)))
                Console.Write("m/s");
            else if (Convert.ToBoolean(ws.unit_settings2 & (1 << 1)))
                Console.Write("km/h");
            else if (Convert.ToBoolean(ws.unit_settings2 & (1 << 2)))
                Console.Write("knot");
            else if (Convert.ToBoolean(ws.unit_settings2 & (1 << 3)))
                Console.Write("m/h");
            else if (Convert.ToBoolean(ws.unit_settings2 & (1 << 4)))
                Console.Write("bft");
            Console.WriteLine("");

            Console.WriteLine("Display settings:");
            Console.WriteLine("  Pressure:\t\t\t{0}", Convert.ToBoolean(ws.display_options1 & (1 << 0)) ? "Relative" : "Absolute");
            Console.WriteLine("  Wind speed:\t\t\t{0}", Convert.ToBoolean(ws.display_options1 & (1 << 1)) ? "Gust" : "Average");
            Console.WriteLine("  Time:\t\t\t\t{0}", Convert.ToBoolean(ws.display_options1 & (1 << 2)) ? "12 hour" : "24 hour");
            Console.WriteLine("  Date:\t\t\t\t{0}", Convert.ToBoolean(ws.display_options1 & (1 << 3)) ? "Month-day-year" : "Day-month-year");
            Console.WriteLine("  Time scale:\t\t\t{0}", Convert.ToBoolean(ws.display_options1 & (1 << 4)) ? "24 hour" : "12 hour");

            Console.Write("  Date:\t\t\t\t");
            if (Convert.ToBoolean(ws.display_options1 & (1 << 5)))
                Console.Write("Show year year");
            else if (Convert.ToBoolean(ws.display_options1 & (1 << 6)))
                Console.Write("Show day name");
            else if (Convert.ToBoolean(ws.display_options1 & (1 << 7)))
                Console.Write("Alarm time");
            Console.WriteLine("");

            Console.Write("  Outdoor temperature:\t\t");
            if (Convert.ToBoolean(ws.display_options2 & (1 << 0)))
                Console.Write("Temperature");
            else if (Convert.ToBoolean(ws.display_options2 & (1 << 1)))
                Console.Write("Wind chill");
            else if (Convert.ToBoolean(ws.display_options2 & (1 << 2)))
                Console.Write("Dew point");
            Console.WriteLine("\n");

            Console.Write("  Rain:\t\t\t\t");
            if (Convert.ToBoolean(ws.display_options2 & (1 << 3)))
                Console.Write("Hour");
            else if (Convert.ToBoolean(ws.display_options2 & (1 << 4)))
                Console.Write("Day");
            else if (Convert.ToBoolean(ws.display_options2 & (1 << 5)))
                Console.Write("Week");
            else if (Convert.ToBoolean(ws.display_options2 & (1 << 6)))
                Console.Write("Month");
            else if (Convert.ToBoolean(ws.display_options2 & (1 << 7)))
                Console.Write("Total");
            Console.WriteLine("\n");
        }

        //
        // Prints a BCD date.
        //
        void print_bcd_date(byte[] date)
        {
            bcd_date_t d = parse_bcd_date(date);
            Console.WriteLine("{0}-{1}-{2} {3}:{4}:00", d.year, d.month, d.day, d.hour, d.minute);
        }

        private DateTime get_timestamp(long timestamp)
        {
            return (new DateTime(1970, 1, 1) + new TimeSpan(0, 0, Convert.ToInt32(timestamp / 1000)));
        }

        private bool has_contact_with_sensor(FOweatherdata wdp)
        {
            return !Convert.ToBoolean((Convert.ToInt32(wdp.status) >> Convert.ToInt32(LOST_SENSOR_CONTACT_BIT)) & 0x1);
        }
    }
}
