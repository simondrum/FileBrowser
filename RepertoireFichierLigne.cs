using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

namespace Proginov.PnvUserControls
{
    [Serializable]
    public class PlkFichier
    {


        //public List<PlkLigne> PNVLignes = new List<PlkLigne>();

        //public List<string> PNVStrLignes = new List<string>();

        private List<string> _PNVStrLignes;
        private string _PNVNom;
        private string _PNVRepertoire;
        private string _PNVExetension;
        public bool PNVChargerLignes;

        private List<string> _pnvStrLignes;
        public List<string> PNVLignes => _pnvStrLignes ?? (_pnvStrLignes = GetLignes());

        private List<string> GetLignes()
        {
            return File.ReadAllLines(PNVRepertoire).ToList();
        }

        public string PNVRepertoire
        {
            get { return _PNVRepertoire; }
            set { _PNVRepertoire = value; }
        }
        public string PNVNom
        {
            get { return _PNVNom; }
            set { _PNVNom = value; }
        }
        public string PNVExtension
        {
            get { return _PNVExetension; }
            set { _PNVExetension = value; }
        }
        public PlkFichier()
            {

            }

        public PlkFichier(string pPath) : this(pPath, false) { }

        public PlkFichier(string pPath, bool pLoadLignesFile)        
        {
            PNVNom = Path.GetFileNameWithoutExtension(pPath);
            PNVRepertoire = Path.GetFullPath(pPath);
            PNVExtension = Path.GetExtension(pPath);
            PNVChargerLignes = pLoadLignesFile;

            if (PNVChargerLignes) this.PNVLoadLignesFile();
        }

        public void PNVLoadLignesFile()
        {
            /*
            string[] LignesLues = File.ReadAllLines(PNVRepertoire);
            PNVStrLignes = LignesLues.ToList();
            foreach (string vLignes in LignesLues)
            {
                PNVLignes.Add(new PlkLigne(vLignes));
            }*/
        }
    }

    [Serializable]
    public class PlkRepertoire
    {
        public List<PlkRepertoire> PNVRepertoires = new List<PlkRepertoire>();
        public List<PlkFichier> PNVFichiers = new List<PlkFichier>();
        public string PNVNom;
        public bool PNVChargerArborescenceRep;
        public bool PNVChargerListeFichiers;
        public bool PNVChargerLignesFichiers;
        private List<PlkRepertoire> RemoveList = new List<PlkRepertoire>();
        private List<PlkFichier> NonSourceFiles = new List<PlkFichier>();
        private bool _NonSourceFilesVisible;

        public PlkRepertoire(string pNom) : this(pNom,false) {}

        public PlkRepertoire(string pNom, bool pLoadSubDirectory) : this(pNom, pLoadSubDirectory, false) { }

        public PlkRepertoire(string pNom, bool pLoadSubDirectory, bool pLoadListeFiles) : this(pNom, pLoadSubDirectory, pLoadListeFiles, false) { }

        public PlkRepertoire()
        {
        }

        public PlkRepertoire(string pNom, bool pLoadSubDirectory, bool pLoadListeFiles, bool pLoadLignesFichiers)
        {
            PNVNom = pNom;
            PNVChargerListeFichiers = pLoadListeFiles;
            PNVChargerArborescenceRep = pLoadSubDirectory;
            PNVChargerLignesFichiers = pLoadLignesFichiers;
            PNVLoadFiles(PNVNom);
        }

        public void PNVLoadFiles()
        {
            //Console.WriteLine("pnvLoadFies: " + PNVNom  );
            if (Directory.Exists(PNVNom))
            {             
                if (PNVChargerListeFichiers == true)
                { 
                string[] fileEntries = Directory.GetFiles(PNVNom);
                foreach (string fileName in fileEntries)
                { 
                    PNVAddFichier(fileName);
                }
                }
                if (PNVChargerArborescenceRep == true)
                { 
                    string[] subdirectoryEntries = Directory.GetDirectories(PNVNom);
                    foreach (string subdirectory in subdirectoryEntries)
                    {
                        PNVAddRepertoire(subdirectory);
                    }
                }
            }
            
        }

        public bool IsEmpty()
        {
            return (this.PNVFichiers.Count == 0 && this.PNVRepertoires.Count == 0);
        }

        public bool NonSourceFilesVisible
        {
            get
            {
                return _NonSourceFilesVisible;
            }
            set
            {
                    _NonSourceFilesVisible = value;
                    this.PNVClearNonSourceFiles();
            }
        }

        /*
        public void ToXml(XmlWriter writer)
        {
            this.ToXml(writer,this.PNVRepertoires);
        }
        public void ToXml(XmlWriter writer, List<PlkRepertoire> ListeReps)
        {
            foreach (PlkRepertoire rep in ListeReps)
            {
                writer.WriteStartElement("Repertoire");
                writer.WriteElementString("Nom", rep.PNVNom);
                ToXml(writer, rep.PNVRepertoires);
                foreach (PlkFichier file in rep.PNVFichiers)
                {
                    writer.WriteStartElement("Fichier");
                    writer.WriteElementString("NomFichier", file.PNVNom);
                    writer.WriteElementString("ExtFichier", file.PNVExtension);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }
        */


        public void PNVClearNonSourceFiles()
        {
            foreach (PlkRepertoire SousRep in this.PNVRepertoires)
            {
                SousRep.PNVClearNonSourceFiles();
            }
            foreach (PlkFichier FileASuppr in NonSourceFiles)
            {
                if (NonSourceFilesVisible)
                {
                    if (PNVFichiers.Contains(FileASuppr) != true) PNVFichiers.Add(FileASuppr);
                }
                else
                {
                    PNVFichiers.Remove(FileASuppr);
                }                
            }            
        }

        public bool PNVClearEmptyFolders ()
        {
            this.RemoveList.Clear();
            foreach (PlkRepertoire SousRep in this.PNVRepertoires)
            {
                if (SousRep.PNVClearEmptyFolders()) RemoveList.Add(SousRep);
            }
            foreach (PlkRepertoire RepASuppr in RemoveList)
            {
                PNVRepertoires.Remove(RepASuppr);
            }
            return this.IsEmpty();
        }
       

        public void PNVLoadFiles(string pRepertoire)
        {
            //if (pRepertoire.Contains(".xml")) PNVLoadFromXml(pRepertoire);            
            PNVNom = pRepertoire;
            this.PNVLoadFiles();
        }

        public void PNVToXml(string pathAndName)
        {
            XmlSerializer xs = new XmlSerializer(typeof(PlkRepertoire));
            TextWriter tw = new StreamWriter(pathAndName);
            xs.Serialize(tw, this);
        }

        public void PNVToBinary(string pathAndName)
        {
            using (Stream stream = File.Open(pathAndName, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, this);
                stream.Close();
            }
        }

        public static PlkRepertoire PNVFromBinary (string pathandname)
        {
            using (Stream stream = File.Open(pathandname, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                return (PlkRepertoire)binaryFormatter.Deserialize(stream);
            }
        }


        public static PlkRepertoire PNVLoadFromXml(string pXml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(PlkRepertoire));
            using (var sr = new StreamReader(pXml))
            {
                return  (PlkRepertoire)xs.Deserialize(sr);
            }
        }

        public void PNVAddRepertoire(string path)
        {
            this.PNVRepertoires.Add(new PlkRepertoire(path, PNVChargerArborescenceRep, PNVChargerListeFichiers, PNVChargerLignesFichiers));            
        }

        // Insert logic for processing found files here.
        protected void PNVAddFichier(string path)
        {
            PlkFichier vFile = new PlkFichier(path, PNVChargerLignesFichiers);
            if (vFile.PNVExtension != ".p" && vFile.PNVExtension != ".i" && vFile.PNVExtension != ".cls" && vFile.PNVExtension != ".lab" && vFile.PNVExtension != ".w" && vFile.PNVExtension != ".lep") this.NonSourceFiles.Add(vFile);
            this.PNVFichiers.Add(vFile);
        }

    }

    /*
    [Serializable]
    public class PlkLigne
    {
        public bool PNVIsEmpty;
        public string PNVLigne;

        public PlkLigne()
        {

        }
            

        public PlkLigne(string pCOntenu)
        {
            PNVLigne = pCOntenu;
            if (PNVLigne == "") PNVIsEmpty = true;
            else PNVIsEmpty = false;
        }
    }
    */
}
