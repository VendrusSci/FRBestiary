﻿using Bestiary.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bestiary.ViewModel
{
    class FetchUpdateViewModel : INotifyPropertyChanged
    {
        string m_FRDataPath;
        public FetchUpdateViewModel(IModel model, string FRDataPath)
        {
            m_LocalFamiliarModel = model;
            m_FRDataPath = FRDataPath;
        }

        public string StatusString { get; private set; }

        private LambdaCommand m_FetchUpdate;
        private bool m_IsBusy = false;
        public ICommand FetchUpdate
        {
            get
            {
                if (m_FetchUpdate == null)
                {
                    m_FetchUpdate = new LambdaCommand(
                        onExecute: (p) =>
                        {
                            Task.Run(() =>
                            {
                                m_IsBusy = true;
                                MainViewModel.UserActionLog.Info("Familiar update process started");
                                StatusString = "Fetching updated familiar list...";

                                if (FetchUpdateFile())
                                {
                                    StatusString = "Familiar list found, updating local file...";
                                    UpdateLocalFile();
                                    MainViewModel.UserActionLog.Info("Deleting local file");
                                    File.Delete(m_LocalUpdateFilePath);
                                }
                                else
                                {
                                    StatusString = "Can't find file to update from";
                                }

                                m_IsBusy = false;
                            });
                        },
                        onCanExecute: (p) =>
                        {
                            return !m_IsBusy;
                        }
                    );
                }
                return m_FetchUpdate;
            }
        }

        private LambdaCommand m_FetchUpdateOverwrite;
        public ICommand FetchUpdateOverwrite
        {
            get
            {
                if(m_FetchUpdateOverwrite == null)
                {
                    m_FetchUpdateOverwrite = new LambdaCommand(
                        onExecute: (p) =>
                        {
                            Task.Run(() =>
                            {
                                if (FetchUpdateFile())
                                {
                                    MainViewModel.UserActionLog.Info("Starting fast update");
                                    if(!StructuralComparisons.StructuralEqualityComparer.Equals(GetHashValue(m_LocalUpdateFilePath), GetHashValue(m_FRDataPath)))
                                    {
                                        StatusString = "Familiar list found, updating local file...";
                                        string bkupFilePath = "Backup.xml";
                                        File.Replace(m_LocalUpdateFilePath, m_FRDataPath, bkupFilePath);
                                        File.Delete(bkupFilePath);
                                        StatusString = "Update Complete";
                                        MainViewModel.UserActionLog.Info("Fast update complete");
                                    }
                                    else
                                    {
                                        StatusString = "Already up to date!";
                                        MainViewModel.UserActionLog.Info("File up to date, no update required");
                                    }
                                    File.Delete(m_LocalUpdateFilePath);
                                }
                            });
                        }
                    );
                }
                return m_FetchUpdateOverwrite;
            }
        }

        private IModel m_LocalFamiliarModel;
        private IModel m_UpdateFamiliarModel;

        private const string m_RemoteUpdateFilePath = "https://raw.githubusercontent.com/VendrusSci/UnofficialFRBestiaryCompanion/master/Bestiary/Resources/FRData.xml";
        private const string m_LocalUpdateFilePath = "SourceFamiliars.xml";

        public event PropertyChangedEventHandler PropertyChanged;

        private bool FetchUpdateFile()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    MainViewModel.UserActionLog.Info($"Attempting to download file with following url: {m_RemoteUpdateFilePath}");
                    client.DownloadFile(m_RemoteUpdateFilePath, m_LocalUpdateFilePath);
                }
                catch(WebException ex)
                {
                    StatusString = "Failed to download file";
                    MainViewModel.UserActionLog.Error("Failed to download file");
                }
            }
            if(File.Exists(m_LocalUpdateFilePath))
            {
                MainViewModel.UserActionLog.Info("Update file download succeeded");
                return true;
            }
            else
            {
                return false;
            }
            
        }

        private void UpdateLocalFile()
        {
            MainViewModel.UserActionLog.Info("Starting update of FR data...");
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(GetHashValue(m_LocalUpdateFilePath), GetHashValue(m_FRDataPath)))
            {
                //if the hashes aren't the same, make a new model and compare all the familiars
                m_UpdateFamiliarModel = new XmlModelStorage(m_LocalUpdateFilePath, "", "");
                foreach(var familiar in m_UpdateFamiliarModel.Familiars)
                {
                    var localFamiliar = m_LocalFamiliarModel.LookupFamiliar(familiar);
                    if (localFamiliar != null)
                    {
                        localFamiliar.Delete();
                    }
                    m_LocalFamiliarModel.AddFamiliar(m_UpdateFamiliarModel.LookupFamiliar(familiar).Fetch());
                }
                MainViewModel.UserActionLog.Info("Update completed");
                StatusString = "Update complete!";
            }
            else
            {
                MainViewModel.UserActionLog.Info("Hash match, no update required");
                StatusString = "Already up to date!";
            }
        }

        private byte[] GetHashValue(string filePath)
        {
            using (var filestream = new FileStream(filePath, FileMode.Open))
            {
                var sha = SHA256.Create();
                return sha.ComputeHash(filestream);
            }
        }
    }
}
