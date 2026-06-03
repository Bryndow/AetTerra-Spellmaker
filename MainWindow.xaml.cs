using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FIO_zeitverteib
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public class Spell
    {
        string name = string.Empty;
        int drain = 0;
        string baseName = string.Empty;
        List<int> sequence = new List<int>();
        bool useBase = false;
        bool continuous = false;
        States allowedStates = 0;
        public enum States { either, air, ground, differ}

        public Spell(string name,int drain, string baseName, List<int> sequence, bool useBase, bool continuous, States allowedStates)
        {
            this.name = name;
            this.drain = drain;
            this.baseName = baseName;
            this.sequence = sequence;
            this.useBase = useBase;
            this.continuous = continuous;
            this.allowedStates = allowedStates;
        }

        public string getName() {  return name; }
        public int getDrain() { return drain; }
        public string getBaseName() { return baseName; }
        public bool isUsingBase() { return useBase; }
        public bool isContinuous() { return continuous; }
        public States getAllowedStates() { return allowedStates; }
        public List<int> getSequence() { return sequence; }

        public string getSeqAsString()
        {
            string store = string.Empty;
            for (int i = 0; i < sequence.Count; i++)
                store += sequence[i].ToString() + ", ";
            return store;
        }
    }

    public class Librarian
    {
        public enum Mode { upgrade, gd}
        Stream? stream;
        string saveDir = string.Empty;

        // Select, has, toss.
        #region
        public bool fileSelect()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            { stream = ofd.OpenFile(); return true; }
            else stream = null;
            return false;
        }
        public void tossFile()
        {
            stream = null;
        }
        public bool hasFile()
        {
            if (stream != null && stream.CanRead && stream.CanWrite)
                return true;
            else return false;
        }
        public bool dirSelect()
        {
            OpenFolderDialog ofd = new OpenFolderDialog();
            if (ofd.ShowDialog() == true)
            { 
                saveDir = ofd.FolderName; 
                return true; 
            }
            else return false;
        }
        public void tossDir()
        {
            saveDir = string.Empty;
        }
        public bool hasDir()
        {
            if (saveDir != string.Empty)
                return true;
            else return false;
        }
        #endregion

        public List<Spell>? readFile()
        {
            string raw = string.Empty;
            List<Spell> store = new List<Spell>();

            string storeName = string.Empty;
            string storeDrain = string.Empty;
            string storeBaseName = string.Empty;
            bool storeUseBase = false;
            bool storeContinuous = false;
            string storeSeqSingle = string.Empty;
            Spell.States storeState = 0;
            List<int> storeSeq = new List<int>();

            bool abort = false;
            if (!hasFile())
            {
                if (!fileSelect())
                {
                    abort = true;
                }
            }

            if (!abort)
            {
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
                StreamReader sr = new StreamReader(stream);
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
                raw = sr.ReadToEnd();
                sr.Close();
                int step = 0;

                //Begin read:
                for (int i = 0; raw.Length > i; i++)
                {
                    if (raw[i] == ',')
                    {
                        step += 1;
                    }
                    else if (raw[i] == ';')
                    {
                        //Add
                        int tmp = 0;
                        if (storeDrain.Length > 0)
                            tmp = Convert.ToInt32(storeDrain);
                        store.Add(new Spell(storeName, tmp, storeBaseName, storeSeq, storeUseBase, storeContinuous, storeState));
                        //Toss all values
                        storeName = string.Empty;
                        storeDrain = string.Empty;
                        storeBaseName = string.Empty;
                        storeSeq = new List<int>();
                        storeUseBase = false;
                        storeContinuous = false;
                        storeState = 0;
                        step = 0;
                    }
                    else if (raw[i] != ';' && raw[i] != ',')
                    {
                        switch (step)
                        {
                            case 0: storeName += raw[i]; break;
                            case 1: storeDrain += raw[i]; break;
                            case 2: storeBaseName += raw[i]; break;
                            case 3:
                                if (raw[i] != '.')
                                    storeSeqSingle += raw[i];
                                else
                                {
                                    storeSeq.Add(Convert.ToInt16(storeSeqSingle));
                                    storeSeqSingle = string.Empty;
                                }
                                break;
                            case 4: if (raw[i] == '1') storeUseBase = true; break;
                            case 5: if (raw[i] == '1') storeContinuous = true; break;
                            case 6: if (raw[i] == '0') storeState = Spell.States.either; if (raw[i] == '1') storeState = Spell.States.air; 
                                if (raw[i] == '2') storeState = Spell.States.ground; if (raw[i] == '3') storeState = Spell.States.differ;
                                break;
                        }
                    }
                }

                return store;
            }
            else return null;
        }

        public bool writeFile(Mode m, List<Spell> lib)
        {
            if (m == Mode.upgrade)
            {
                if (stream != null && stream.CanWrite)
                {
                    StreamWriter swr = new StreamWriter(stream);
                    // Writing Section
                    string write = string.Empty;
                    for (int i = 0; i < lib.Count; i++)
                    {
                        Spell c = lib[i];
                        write += c.getName() + "," + c.getBaseName() + "," + c.getSeqAsString() + ",";
                        if (c.isUsingBase()) write += "1,";
                        else write += "0,";
                        if (c.isContinuous()) write += "1,";
                        else write += "0,";
                        switch (c.getAllowedStates())
                        {
                            case Spell.States.either: write += "0;"; break;
                            case Spell.States.air: write += "1;"; break;
                            case Spell.States.ground: write += "2;"; break;
                            case Spell.States.differ: write += "3;"; break;
                        }
                        write += "\n";
                    }
                    swr.Write(write);
                    swr.Close();
                    // end of Writing Section
                    return true;
                    }
                }
            if (m == Mode.gd)
            {
                bool abort = false;
                if (saveDir == string.Empty)
                    if (!dirSelect())
                        abort = true;

                if (!abort)
                {
                    StreamWriter control = new StreamWriter(saveDir + "control.txt");
                    StreamWriter init = new StreamWriter(saveDir + "init.txt");
                    StreamWriter append = new StreamWriter(saveDir + "append.txt");
                    string cStore = string.Empty;
                    string iStore = string.Empty;
                    string aStore = string.Empty;
                    for (int i = 0; i < lib.Count; i++)
                    {
                        Spell c = lib[i];

                        // control
                        switch (c.isContinuous())
                        {
                            case true: cStore += "if spell == " + c.getName() + " && mana >= " + c.getName() + ".drain:"; break;
                            case false: cStore += "if spell == " + c.getName() + " && mana >= " + c.getName() + ".drain || spell == " + c.getName() + " && mana > 10:"; break;
                        }
                        cStore += "\n\t";

                        switch (c.getAllowedStates())
                        {
                            case Spell.States.air: cStore += "if !body.is_on_floor(): \n\t\t"; break;
                            case Spell.States.ground: cStore += "if body.is_on_floor(): \n\t\t"; break;
                            case Spell.States.differ:
                                cStore += "if body.is_on_floor(): \n\t\t\n\n" +
                                    "\tif !body.is_on_floor(): \n\t\t"; break;
                        }
                        cStore += "\n\n";

                        // init
                        List<int> t = c.getSequence();
                        iStore += "var " + c.getName() + " = Spell.new(\"" + c.getName() + "\"," + c.getDrain() + "[";
                        for (int j = 0; t.Count() > j; j++)
                        {
                            iStore += t[j].ToString();
                            if (j != t.Count())
                                iStore += ",";
                        } // adds sequence
                        iStore += "],";
                        switch (c.isUsingBase())
                        {
                            case true: iStore += "true"; break;
                            case false: iStore += "false"; break;
                        }
                        switch (c.isContinuous())
                        {
                            case true: iStore += "true"; break;
                            case false: iStore += "false"; break;
                        }
                        iStore += ")\n";
                    }
                    control.Write(cStore);
                    init.Write(iStore);

                    return true;
                }
                return false;
            }
            return false;
        }
    }

    public partial class MainWindow : Window
    {
        List<int> seq = new List<int>();
        Librarian carl = new Librarian(); /// Carl gives us the detes. We don't question Carl, he's the only one taking notes.
        List<Spell> spellCache = new List<Spell>();
        public MainWindow()
        {
            InitializeComponent();
        }

        public void debug_display()
        {
            debug_curentSeqDisplay.Items.Clear();
            debug_showSpells.Items.Clear();
            for (int i=0; i<seq.Count; i++)
            {
                debug_curentSeqDisplay.Items.Add(seq[i]);
            }
        }

        public void update()
        {
            debug_display();
        }

        public List<Spell> matchAndReplace(List<Spell> toAdd, List<Spell> baseList)
        {
            for (int i=0;i<toAdd.Count;i++)
            {
                for (int j=0;j<baseList.Count;j++)
                    if (toAdd[i].getName() == baseList[j].getName())
                    {
                        toAdd[i] = baseList[j];
                    }
                if (baseList.Contains(toAdd[i]))
                    {toAdd.Remove(toAdd[i]); i = 0; }
                else
                    baseList.Add(toAdd[i]);
            }
            return baseList;
        }

        /*
         * Base Inputs
         */
        #region
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            seq.Clear();
        }

        private void btnSelFile_Click(object sender, RoutedEventArgs e)
        {
            carl.fileSelect();
        }

        private void btnWrite_Click(object sender, RoutedEventArgs e)
        {
            List<Spell>? t = null;
            t = carl.readFile();
            if (t != null)
            {
                t = matchAndReplace(spellCache, t);
                carl.writeFile(Librarian.Mode.upgrade, t);
            }
            else
                MessageBox.Show("No source file.");
        }

        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            carl.readFile();
            debug_display();
        }

        private void btnPassIn_Click(object sender, RoutedEventArgs e)
        {
            Spell.States cStates = Spell.States.either;
            if (cbStateRegardless.IsChecked == true) cStates = Spell.States.either;
            if (cbStateAir.IsChecked == true) cStates = Spell.States.air;
            if (cbStateGround.IsChecked == true) cStates = Spell.States.ground;
            if (cbStateBoth.IsChecked == true) cStates = Spell.States.differ;


            if (cbUseBase.IsChecked != null && cbContinuous.IsChecked != null)
                spellCache.Add(new Spell(tbName.Text, Convert.ToInt32(slDrain.Value), tbBaseName.Text, seq, (bool)cbUseBase.IsChecked, (bool)cbContinuous.IsChecked, cStates));

            seq = new List<int>();
            spellCache = new List<Spell>();

            // Display currently in-file spells' names
            debug_showSpells.Items.Clear();
            List<Spell>? rs = carl.readFile();
            if (rs != null)
            {
                for (int i = 0; rs.Count > i; i++)
                {
                    debug_showSpells.Items.Add(rs[i].getName());
                }
            }
        }

        private void btnTossFile_Click(object sender, RoutedEventArgs e)
        {
            carl.tossFile();
            carl.tossDir();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        // Matrix Clickevents
        #region
        private void btnS_MIN_1_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(11);
            update();
        }

        private void btnS_MIN_2_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(12);
            update();
        }

        private void btnS_MIN_3_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(13);
            update();
        }

        private void btnS_MIN_4_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(14);
            update();
        }

        private void btnS_MIN_5_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(15);
            update();
        }

        private void btnS_MIN_6_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(16);
            update();
        }

        private void btnS_MIN_7_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(17);
            update();
        }

        private void btnS_MIN_8_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(18);
            update();
        }

        private void btnS_MID_1_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(21);
            update();
        }

        private void btnS_MID_2_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(22);
            update();
        }

        private void btnS_MID_3_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(23);
            update();
        }

        private void btnS_MID_4_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(24);
            update();
        }

        private void btnS_MID_5_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(25);
            update();
        }

        private void btnS_MID_6_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(26);
            update();
        }

        private void btnS_MID_7_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(27);
            update();
        }

        private void btnS_MID_8_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(28);
            update();
        }

        private void btnS_MAX_1_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(31);
            update();
        }

        private void btnS_MAX_2_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(32);
            update();
        }

        private void btnS_MAX_3_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(33);
            update();
        }

        private void btnS_MAX_4_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(34);
            update();
        }

        private void btnS_MAX_5_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(35);
            update();
        }

        private void btnS_MAX_6_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(36);
            update();
        }

        private void btnS_MAX_7_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(37);
            update();
        }

        private void btnS_MAX_8_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(38);
            update();
        }
        #endregion
    }
}