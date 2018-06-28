using System;
using System.Linq;
using System.IO;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using System.Collections.Generic;


namespace BSky.Statistics.Common
{
    public class Journal : ILogDevice
    {
        ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();//17Dec2012
        private string _fileName;//fullpathfilename
        private int _maxfilesize;
        private int _maxbackupfiles;

        public Journal()
        {
            GetAllBackgroundCommands();
        }

        #region ILogDevice Members

        public void WriteLine(string text)
        {
            if (!String.IsNullOrEmpty(_fileName) && null == _writer)
                OpenLogFile(_fileName);

            if (null != _writer)
            {
                if (CheckFileSize(_fileName))
                {
                    DateTime uanow = DateTime.Now;//Added by Anil to get current date time
                    _writer.WriteLine(uanow.ToLocalTime() + " :: " + text);
                }
            }
        }

        //12Aug2016
        public void WriteUserCommands(string text)
        {
            if (IsBackgroundCommand(text))//Dont write background commands to user journal
                return;

            if (text.StartsWith("print("))
            {
                text = removePrint(text).Trim();
                // print is removed, now remove the parenthesis (that was with print)
                if (text.StartsWith("(") && text.EndsWith(")"))//if it has eboth open and close brackets
                {   
                    if(text.Length>2)//length is more than 2 means there is something in parenthesis, else empty parenthesis
                        text = text.Substring(1, text.Length - 2);
                }
            }
            if (text.Contains("BSky.LoadRefresh.Dataframe"))
            {
                text = text.Replace("BSky.LoadRefresh.Dataframe", "BSkyLoadRefreshDataframe").Replace("'", "") ;
            }

            if (!String.IsNullOrEmpty(_fileName) && null == _writer)
                OpenLogFile(_fileName);

            if (null != _writer)
            {
                if (CheckFileSize(_fileName))
                {
                    DateTime uanow = DateTime.Now;//Added by Anil to get current date time
                    _writer.WriteLine(uanow.ToLocalTime() + " :: " + text);
                }
            }
        }
        //12Aug2016 This will do a string comparision to find if the command is one of the background commands
        private bool IsBackgroundCommand(string text)
        {
            bool isBackgroundCommand = false;

            //backgroundCommandList contains the list of background commands from BSkyBgrndComm.lst file
            //Usually background command count is more than 20 (nearly 35)
            if (backgroundCommandList != null && backgroundCommandList.Count > 20)
            {
                if (backgroundCommandList.Any(s => text.Contains(s)))
                {
                    isBackgroundCommand = true;
                }
            }
            else
            {
                //If 'backgroundCommandList' list is null or empty then use following
                if (
                     text.Contains("BSkyGetDecimalDigitSetting(") ||
                     text.Contains("BSkySetDecimalDigitSetting(") ||
                     text.Contains("BSkySetEngNotationSetting(") ||
                     text.Contains("sink(stderr(), type=c(\"message\"))") ||
                     text.Contains("sink(fp, append=FALSE, type=c(\"message\"))") ||
                     text.Contains("sink(fp, append=FALSE, type=c(\"output\"))") ||
                     text.Contains("sink()") ||
                     text.Contains("flush(fp)") ||
                     text.Contains("close(fp)") ||
                     text.Contains("bskytempvarname <- ") ||
                     text.Contains("exists('bskytempvarname')") ||
                     text.Contains("is.null(bskytempvarname)") ||
                     text.Contains("is.null(row.names(bskytempvarname)[1])") ||
                     text.Contains("row.names(bskytempvarname)[1]") ||
                     text.Contains("bskyfrmtobj <- BSkyFormat(bskytempvarname, ") ||
                     text.Contains("is.null(bskyfrmtobj)") ||
                     text.Contains("class(bskyfrmtobj)") ||
                     text.Contains("is.null(bskyfrmtobj$BSkySplit)") ||
                     text.Contains("is.null(bskyfrmtobj$list2name)") ||
                     text.Contains("bskyfrmtobj$executionstatus") ||
                     text.Contains("bskyfrmtobj$nooftables") ||
                     text.Contains("is.null(bskyfrmtobj$BSkySplit)") ||
                     text.Contains("bskyfrmtobj$uasummary[[7]]") ||
                     text.Contains("bskyfrmtobj") ||
                     text.Contains("fp<- file(") && text.Contains("rsink.txt") ||
                     text.Contains("options('warn'=1)") ||
                     text.Contains("BSkyBatchCommand(1)") ||
                     text.Contains("BSkySetCurrentDatasetName(") ||
                     text.Contains("New.version.BSkyComputeSplitdataset(") ||
                     text.Contains("BSkyBatchCommand(0)") ||
                     text.Contains("tiscuni <- exists('bskytempvarname')") ||
                     text.Contains("rm('bskyfrmtobj')") ||
                     text.Contains("rm('bskytempvarname')") ||
                     text.Contains("png(") && text.Contains("image%03d.png") ||
                     text.Contains("if(dev.cur()[[1]] == 2) dev.off()") ||
                     text.Contains("bskyattr <-") ||
                     text.Contains("<- bskyattr")
                    //text.Contains("BSky.LoadRefresh.Dataframe") ||
                    //text.Contains("bskyfrmtobj") 
                    )
                {
                    isBackgroundCommand = true;
                }
            }
            

            return isBackgroundCommand;
        }
        //12Aug2016 remove 'print'. We still have parenthesis those were with 'print'. We can enhance this later for removing parenthesis.
        private string removePrint(string txt)
        {
            return txt.Replace("print","");
        }

        //Follwing method creates a list of background commands
        //These commands will be compared and will not be sent to user journal
        //User journal will only contain the commands that user exeutes either using dialog or from syntax
        List<string> backgroundCommandList = null;
        char autoCSeparator = ' ';//space is default separator but is not good when you have multi-word keywords
        private void GetAllBackgroundCommands()
        {
            string autocomplete = string.Empty;
            backgroundCommandList = new List<string>();

            //Read text file 1 line is one entry for autocomplete list
            int counter = 0;
            string line;
            bool isComment = false, isSeparator = false;
            // Read the file and display it line by line.
            if (!File.Exists(".\\BSkyBGrndCmd.lst"))
                return;
            System.IO.StreamReader file = new System.IO.StreamReader(".\\BSkyBGrndCmd.lst");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Trim().StartsWith("#")) // this line has comments
                {
                    isComment = true;
                }
                else if (line.Trim().StartsWith("$")) // character after $ is a character that will be used for separator
                {
                    if (line.Length > 1)//must exactly have 2 chars: $ and separator character.
                    {
                        autoCSeparator = line.ToCharArray()[1];// index 0 is $ while next char would be separator
                    }
                    isSeparator = true;
                }
                else
                {
                    isComment = false;
                    isSeparator = false;
                }

                //if its not separator or comment line then add to the list
                if (!isComment && !isSeparator)
                {
                    if (!backgroundCommandList.Contains(line.Trim()))//no duplicates are added.
                    {
                        backgroundCommandList.Add(line.Trim());
                    }
                }
                //Console.WriteLine(line);
                counter++;
            }

            file.Close();

            //sort items in templist
            //backgroundCommandList.Sort();
        }

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
        public int MaxFileSize
        {
            get { return _maxfilesize; }
            set { _maxfilesize = value; }
        }
        public int MaxBackupFiles
        {
            get { return _maxbackupfiles; }
            set { _maxbackupfiles = value; }
        }

        #endregion
        StreamWriter _writer;
        private void OpenLogFile(string fileName)
        {
            Close();

            string dirPath = Path.GetDirectoryName(fileName);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            _writer = File.AppendText(fileName);
            _writer.AutoFlush = true;

        }
        private void Close()
        {
            if (null == _writer) return;

            _writer.Close();
            _writer = null;
        }

        // check R Log filesize and if its more than say 500KB then rename 
        // current log for backup and create new log file with same name as current log.
        // If Max number of backup files is reached then start deleting oldest & create backup
        // of current file with same name.
        private bool CheckFileSize(string rlogfname)//AD
        {
            int maxFilesizeinKB = 50;
            int maxBackupFiles = 10;
            string bkupfname = string.Empty;//rlogfname + ".1";

            //// Backup logic ///
            try
            {
                if (File.Exists(rlogfname))
                {
                    FileInfo f = new FileInfo(rlogfname);
                    long s1 = f.Length;
                    if (s1 > maxFilesizeinKB * 1024)// Max file size 
                    {

                        /// Generate Backup filename ///
                        for (int i = 1; i <= maxBackupFiles; i++)
                        {
                            if (!File.Exists(rlogfname + "." + i.ToString()))//if file does not exist
                            {
                                bkupfname = rlogfname + "." + i.ToString();
                                break;
                            }
                        }
                        if (bkupfname.Trim().Length < 1)//No name got generated means there are already max backup files.
                        {
                            ////delete oldest and use that name for new backup file. ie delete .1 then .2 then .3
                            string dirname = Path.GetDirectoryName(rlogfname);
                            DirectoryInfo dirInfo = new DirectoryInfo(dirname);
                            string serachpattern = Path.GetFileNameWithoutExtension(rlogfname)+"*";// needed because we have BSkyJournal and BSkyUserJournal in the same folder.
                            FileInfo[] allFiles = dirInfo.GetFiles(serachpattern); //search for only the matching journal.
                            if (allFiles.Length == 0)
                                return false;

                            FileInfo oldestfile = allFiles[0];
                            foreach (var currfile in allFiles.Skip(1))
                            {
                                if (currfile.LastWriteTime < oldestfile.LastWriteTime)
                                    oldestfile = currfile;
                            }
                            bkupfname = oldestfile.FullName;
                        }

                        /// close current log file///
                        Close();

                        /// delete old backup file ///
                        File.Delete(bkupfname);

                        /// rename current log file to backup filename ///
                        File.Move(rlogfname, bkupfname);

                        /// create and open new log file ///
                        OpenLogFile(rlogfname);
                    }

                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                logService.WriteToLogLevel(ex.Message, LogLevelEnum.Error);
                return false;
            }
            return true;
        }

    }
}
