using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Annotations.Storage;
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
        bool useTrgt = false;
        public enum States { either, air, ground, differ}

        public Spell(string name,int drain, string baseName, List<int> sequence, bool useBase, bool continuous, States allowedStates, bool useTrgt)
        {
            // Linebreak check. 1st level, for cleaner data. Effctivley removes \n and \r if present.
            // WARNING: Removes the first char, (or first 2 chars) and not necessarily the operators themselves!!!
            string nStore = name;
            if (nStore.Contains("\r\n"))
            {
                nStore = nStore.Remove(0,2);
            }
            else if (nStore.Contains("\n") || nStore.Contains("\r"))
            {
                nStore = nStore.Remove(0, 1);
            }
            this.name = nStore;
            //

            nStore = baseName;
            if (nStore.Contains("\r\n"))
            {
                nStore = nStore.Remove(0, 2);
            }
            else if (nStore.Contains("\n") || nStore.Contains("\r"))
            {
                nStore = nStore.Remove(0, 1);
            }
            this.baseName = nStore;

            this.drain = drain;
            this.sequence = sequence;
            this.useBase = useBase;
            this.continuous = continuous;
            this.allowedStates = allowedStates;
            this.useTrgt = useTrgt;
        }

        public string getName() {  return name; }
        public int getDrain() { return drain; }
        public string getBaseName() { return baseName; }
        public bool isUsingBase() { return useBase; }
        public bool isContinuous() { return continuous; }
        public States getAllowedStates() { return allowedStates; }
        public List<int> getSequence() { return sequence; }
        public bool isUsingTrgt() { return useTrgt; }
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
        string saveFileDir = string.Empty;
        string saveDir = string.Empty;

        // Select, has, toss.
        #region
        public bool fileSelect()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                saveFileDir = ofd.FileName;
                return true; }
            else saveFileDir = string.Empty;
            return false;
        }
        public void tossFile()
        {
            saveFileDir = string.Empty;
        }
        // Function is the literal devil. Inefficient as hell, dumb, could've been better.
        // And most importantly, a great lesson. DO NOT USE STREAMWRITER TO CHECK FOR WRITEABILIY. IT WILL DELETE ALL DATA ON FLUSH/CLOSE
        public bool hasFile()
        {
            try
            {
                StreamReader test = new StreamReader(saveFileDir);
                test.Close();
                return true;
            }
            catch
            { return false; }
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
            bool storeUseTrgt = false;
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
                StreamReader sr = new StreamReader(saveFileDir);
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
                        store.Add(new Spell(storeName, tmp, storeBaseName, storeSeq, storeUseBase, storeContinuous, storeState, storeUseTrgt));
                        //Toss all values
                        storeName = string.Empty;
                        storeDrain = string.Empty;
                        storeBaseName = string.Empty;
                        storeSeq = new List<int>();
                        storeUseBase = false;
                        storeContinuous = false;
                        storeState = 0;
                        storeUseTrgt = false;

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
                            case 7: if (raw[i] == '1') storeUseTrgt = true; break;
                        }
                    }
                }

                return store;
            }
            else return null;
        }

        public bool writeFile(Mode m, List<Spell> lib)
        {
            // Called when user presses "save" button. Overwrites the original selected file, with in memory, and new spells.
            if (m == Mode.upgrade)
            {
                if (hasFile())
                {
                    StreamWriter swr = new StreamWriter(saveFileDir);
                    // Writing Section
                    string write = string.Empty;
                    for (int i = 0; i < lib.Count; i++)
                    {
                        Spell c = lib[i]; // Localizes current Spell, for less crowded source code
                        // Name, Drain, Base Name
                        write += c.getName() + "," + c.getDrain().ToString() + "," + c.getBaseName() + ",";
                        // Sequence
                        List<int> locSeq = c.getSequence();
                        for (int j = 0; locSeq.Count() > j; j++)
                        {
                            write += locSeq[j].ToString() + ".";
                        }
                        write += ",";
                        // Base
                        if (c.isUsingBase()) write += "1,";
                        else write += "0,";
                        // Continuous
                        if (c.isContinuous()) write += "1,";
                        else write += "0,";
                        // States
                        switch (c.getAllowedStates())
                        {
                            case Spell.States.either: write += "0"; break;
                            case Spell.States.air: write += "1"; break;
                            case Spell.States.ground: write += "2"; break;
                            case Spell.States.differ: write += "3"; break;
                        }
                        write += ",";
                        // Uses Target
                        if (c.isUsingTrgt()) write += "1";
                        else write += "0";
                        // END
                        write += ";\n";
                    }
                    if (write != string.Empty)
                        swr.Write(write);
                    swr.Close();
                    // end of Writing Section
                    return true;
                    }
                }
            // Called when user presses "export" button. Creates/Overwrites two new files, with in memory, and new spells.
            if (m == Mode.gd)
            {
                bool abort = false;
                if (saveDir == string.Empty)
                    if (!dirSelect())
                        abort = true;

                if (!abort)
                {
                    StreamWriter control = new StreamWriter(saveDir + "\\control.txt");
                    StreamWriter init = new StreamWriter(saveDir + "\\init.txt");
                    StreamWriter append = new StreamWriter(saveDir + "\\append.txt");
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
                        //
                        List<int> t = c.getSequence();
                        iStore += "var " + c.getName() + " = Spell.new(\"" + c.getName() + "\"," + c.getDrain() + ",[";
                        // adds sequence
                        for (int j = 0; t.Count() > j; j++)
                        {
                            iStore += t[j].ToString();
                            if (j != t.Count())
                                iStore += ",";
                        } 
                        iStore += "],";
                        // useBase & continuous
                        switch (c.isUsingBase())
                        {
                            case true: iStore += "true"; break;
                            case false: iStore += "false"; break;
                        }
                        iStore += ",";
                        switch (c.isContinuous())
                        {
                            case true: iStore += "true"; break;
                            case false: iStore += "false"; break;
                        }
                        iStore += ",";
                        // base name, if present
                        if (c.isUsingBase())
                            iStore += "\"" + c.getBaseName() + "\"";
                        else iStore += "\"\"";
                        iStore += ",";
                        // useTrgt
                        switch (c.isUsingTrgt())
                        {
                            case true: iStore += "true"; break;
                            case false : iStore += "false"; break;
                        }

                        iStore += ")\n";

                        // append
                        aStore += "spells.append(" + c.getName() + ")\n"; // now that's what I call a "one-liner"
                    }
                    control.Write(cStore);
                    init.Write(iStore);
                    append.Write(aStore);

                    // Flush (OMG THIS IS SO UNNECESSARY)
                    control.Close();
                    init.Close();
                    append.Close();

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
            update();
        }

        public void debug_display()
        {
            debug_curentSeqDisplay.Items.Clear();
            debug_showSpells.Items.Clear();
            for (int i=0; i<seq.Count; i++)
            {
                debug_curentSeqDisplay.Items.Add(seq[i]);
            }

            List<Spell>? tmpSpells = carl.readFile();
            if (tmpSpells != null)
                for (int i=0;i<tmpSpells.Count;i++)
                    debug_showSpells.Items.Add(tmpSpells[i].getName()+"\t Base: "+ tmpSpells[i].isUsingBase().ToString());
        }

        public void update()
        {
            debug_display();
            if (seq.Count > 0)
            {
                if (seq[seq.Count - 1] > 20)
                    hideRing(2);
                else if (seq[seq.Count - 1] > 30)
                    hideRing(3);
                else
                    hideRing(1);
            }
            else
                hideRing(0);
        }

        // Resets the buttons' availability.
        public void popOutButtons()
        {
            btnS_MIN_1.IsEnabled = true;
            btnS_MIN_2.IsEnabled = true;
            btnS_MIN_3.IsEnabled = true;
            btnS_MIN_4.IsEnabled = true;
            btnS_MIN_5.IsEnabled = true;
            btnS_MIN_6.IsEnabled = true;
            btnS_MIN_7.IsEnabled = true;
            btnS_MIN_8.IsEnabled = true;

            btnS_MID_1.IsEnabled = true;
            btnS_MID_2.IsEnabled = true;
            btnS_MID_3.IsEnabled = true;
            btnS_MID_4.IsEnabled = true;
            btnS_MID_5.IsEnabled = true;
            btnS_MID_6.IsEnabled = true;
            btnS_MID_7.IsEnabled = true;
            btnS_MID_8.IsEnabled = true;

            btnS_MAX_1.IsEnabled = true;
            btnS_MAX_2.IsEnabled = true;
            btnS_MAX_3.IsEnabled = true;
            btnS_MAX_4.IsEnabled = true;
            btnS_MAX_5.IsEnabled = true;
            btnS_MAX_6.IsEnabled = true;
            btnS_MAX_7.IsEnabled = true;
            btnS_MAX_8.IsEnabled = true;

        }

        // Hides non-adjacent rings, to prevent user from creating impossible (from game perspective) sequences. Starting number is 0 (<1)
        public void hideRing(int r)
        {
            if (r < 1)
            {
                btnS_MIN_1.Visibility = Visibility.Visible;
                btnS_MIN_2.Visibility = Visibility.Visible;
                btnS_MIN_3.Visibility = Visibility.Visible;
                btnS_MIN_4.Visibility = Visibility.Visible;
                btnS_MIN_5.Visibility = Visibility.Visible;
                btnS_MIN_6.Visibility = Visibility.Visible;
                btnS_MIN_7.Visibility = Visibility.Visible;
                btnS_MIN_8.Visibility = Visibility.Visible;

                btnS_MID_1.Visibility = Visibility.Hidden;
                btnS_MID_2.Visibility = Visibility.Hidden;
                btnS_MID_3.Visibility = Visibility.Hidden;
                btnS_MID_4.Visibility = Visibility.Hidden;
                btnS_MID_5.Visibility = Visibility.Hidden;
                btnS_MID_6.Visibility = Visibility.Hidden;
                btnS_MID_7.Visibility = Visibility.Hidden;
                btnS_MID_8.Visibility = Visibility.Hidden;

                btnS_MAX_1.Visibility = Visibility.Hidden;
                btnS_MAX_2.Visibility = Visibility.Hidden;
                btnS_MAX_3.Visibility = Visibility.Hidden;
                btnS_MAX_4.Visibility = Visibility.Hidden;
                btnS_MAX_5.Visibility = Visibility.Hidden;
                btnS_MAX_6.Visibility = Visibility.Hidden;
                btnS_MAX_7.Visibility = Visibility.Hidden;
                btnS_MAX_8.Visibility = Visibility.Hidden;
            }
            else if (r == 1)
            {
                btnS_MIN_1.Visibility = Visibility.Visible;
                btnS_MIN_2.Visibility = Visibility.Visible;
                btnS_MIN_3.Visibility = Visibility.Visible;
                btnS_MIN_4.Visibility = Visibility.Visible;
                btnS_MIN_5.Visibility = Visibility.Visible;
                btnS_MIN_6.Visibility = Visibility.Visible;
                btnS_MIN_7.Visibility = Visibility.Visible;
                btnS_MIN_8.Visibility = Visibility.Visible;

                btnS_MID_1.Visibility = Visibility.Visible;
                btnS_MID_2.Visibility = Visibility.Visible;
                btnS_MID_3.Visibility = Visibility.Visible;
                btnS_MID_4.Visibility = Visibility.Visible;
                btnS_MID_5.Visibility = Visibility.Visible;
                btnS_MID_6.Visibility = Visibility.Visible;
                btnS_MID_7.Visibility = Visibility.Visible;
                btnS_MID_8.Visibility = Visibility.Visible;

                btnS_MAX_1.Visibility = Visibility.Hidden;
                btnS_MAX_2.Visibility = Visibility.Hidden;
                btnS_MAX_3.Visibility = Visibility.Hidden;
                btnS_MAX_4.Visibility = Visibility.Hidden;
                btnS_MAX_5.Visibility = Visibility.Hidden;
                btnS_MAX_6.Visibility = Visibility.Hidden;
                btnS_MAX_7.Visibility = Visibility.Hidden;
                btnS_MAX_8.Visibility = Visibility.Hidden;
            }
            else if (r == 2)
            {
                btnS_MIN_1.Visibility = Visibility.Visible;
                btnS_MIN_2.Visibility = Visibility.Visible;
                btnS_MIN_3.Visibility = Visibility.Visible;
                btnS_MIN_4.Visibility = Visibility.Visible;
                btnS_MIN_5.Visibility = Visibility.Visible;
                btnS_MIN_6.Visibility = Visibility.Visible;
                btnS_MIN_7.Visibility = Visibility.Visible;
                btnS_MIN_8.Visibility = Visibility.Visible;

                btnS_MID_1.Visibility = Visibility.Visible;
                btnS_MID_2.Visibility = Visibility.Visible;
                btnS_MID_3.Visibility = Visibility.Visible;
                btnS_MID_4.Visibility = Visibility.Visible;
                btnS_MID_5.Visibility = Visibility.Visible;
                btnS_MID_6.Visibility = Visibility.Visible;
                btnS_MID_7.Visibility = Visibility.Visible;
                btnS_MID_8.Visibility = Visibility.Visible;

                btnS_MAX_1.Visibility = Visibility.Visible;
                btnS_MAX_2.Visibility = Visibility.Visible;
                btnS_MAX_3.Visibility = Visibility.Visible;
                btnS_MAX_4.Visibility = Visibility.Visible;
                btnS_MAX_5.Visibility = Visibility.Visible;
                btnS_MAX_6.Visibility = Visibility.Visible;
                btnS_MAX_7.Visibility = Visibility.Visible;
                btnS_MAX_8.Visibility = Visibility.Visible;
            }
            else if (r == 3)
            {
                btnS_MIN_1.Visibility = Visibility.Hidden;
                btnS_MIN_2.Visibility = Visibility.Hidden;
                btnS_MIN_3.Visibility = Visibility.Hidden;
                btnS_MIN_4.Visibility = Visibility.Hidden;
                btnS_MIN_5.Visibility = Visibility.Hidden;
                btnS_MIN_6.Visibility = Visibility.Hidden;
                btnS_MIN_7.Visibility = Visibility.Hidden;
                btnS_MIN_8.Visibility = Visibility.Hidden;

                btnS_MID_1.Visibility = Visibility.Visible;
                btnS_MID_2.Visibility = Visibility.Visible;
                btnS_MID_3.Visibility = Visibility.Visible;
                btnS_MID_4.Visibility = Visibility.Visible;
                btnS_MID_5.Visibility = Visibility.Visible;
                btnS_MID_6.Visibility = Visibility.Visible;
                btnS_MID_7.Visibility = Visibility.Visible;
                btnS_MID_8.Visibility = Visibility.Visible;

                btnS_MAX_1.Visibility = Visibility.Visible;
                btnS_MAX_2.Visibility = Visibility.Visible;
                btnS_MAX_3.Visibility = Visibility.Visible;
                btnS_MAX_4.Visibility = Visibility.Visible;
                btnS_MAX_5.Visibility = Visibility.Visible;
                btnS_MAX_6.Visibility = Visibility.Visible;
                btnS_MAX_7.Visibility = Visibility.Visible;
                btnS_MAX_8.Visibility = Visibility.Visible;
            }
        }

        // Surprisingly enough, competely functional.
        // Checks for Spells with the same "name" property, and when finding one, replaces it with the latest instance, regardless of wether the data qualifies.
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
            popOutButtons();
            update();
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
                spellCache = new List<Spell>();
                carl.writeFile(Librarian.Mode.upgrade, t);
            }
            else
                MessageBox.Show("No source file.");
            update();
        }

        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            carl.readFile();
            update();
        }

        // Functional, with a few checks, to guarantee a minimum amount of data is present
        private void btnPassIn_Click(object sender, RoutedEventArgs e)
        {
            if (seq.Count > 0)
            {
                if (tbName.Text.Length > 0)
                {
                    Spell.States cStates = Spell.States.either;
                    if (cbStateRegardless.IsChecked == true) cStates = Spell.States.either;
                    if (cbStateAir.IsChecked == true) cStates = Spell.States.air;
                    if (cbStateGround.IsChecked == true) cStates = Spell.States.ground;
                    if (cbStateBoth.IsChecked == true) cStates = Spell.States.differ;


                    if (cbUseBase.IsChecked != null && cbContinuous.IsChecked != null && cbUseTrgt.IsChecked != null)
                        spellCache.Add(new Spell(tbName.Text, Convert.ToInt32(slDrain.Value), tbBaseName.Text, seq, (bool)cbUseBase.IsChecked, (bool)cbContinuous.IsChecked, cStates, (bool)cbUseTrgt.IsChecked));

                    seq = new List<int>();
                    popOutButtons();
                }
                else
                    MessageBox.Show("This Spell has no name. The name field MUST be filled.");
            }
            else
                MessageBox.Show("The current Spell has no sequence.");

            update();
        }

        private void btnTossFile_Click(object sender, RoutedEventArgs e)
        {
            carl.tossFile();
            carl.tossDir();
            seq = new List<int>();
            popOutButtons();
            update();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            List<Spell>? t = null;
            t = carl.readFile();
            if (t != null)
            {
                t = matchAndReplace(spellCache, t);
                carl.writeFile(Librarian.Mode.gd, t);
                seq.Clear();
                popOutButtons();
            }
            else
                MessageBox.Show("No source file.");
            update();
        }
        #endregion

        // Matrix Clickevents
        #region
        private void btnS_MIN_1_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(11);
            btnS_MIN_1.IsEnabled = false;
            update();
        }

        private void btnS_MIN_2_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(12);
            btnS_MIN_2.IsEnabled = false;
            update();
        }

        private void btnS_MIN_3_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(13);
            btnS_MIN_3.IsEnabled = false;
            update();
        }

        private void btnS_MIN_4_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(14);
            btnS_MIN_4.IsEnabled = false;
            update();
        }

        private void btnS_MIN_5_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(15);
            btnS_MIN_5.IsEnabled = false;
            update();
        }

        private void btnS_MIN_6_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(16);
            btnS_MIN_6.IsEnabled = false;
            update();
        }

        private void btnS_MIN_7_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(17);
            btnS_MIN_7.IsEnabled = false;
            update();
        }

        private void btnS_MIN_8_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(18);
            btnS_MIN_8.IsEnabled = false;
            update();
        }

        private void btnS_MID_1_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(21);
            btnS_MID_1.IsEnabled = false;
            update();
        }

        private void btnS_MID_2_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(22);
            btnS_MID_2.IsEnabled = false;
            update();
        }

        private void btnS_MID_3_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(23);
            btnS_MID_3.IsEnabled = false;
            update();
        }

        private void btnS_MID_4_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(24);
            btnS_MID_4.IsEnabled = false;
            update();
        }

        private void btnS_MID_5_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(25);
            btnS_MID_5.IsEnabled = false;
            update();
        }

        private void btnS_MID_6_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(26);
            btnS_MID_6.IsEnabled = false;
            update();
        }

        private void btnS_MID_7_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(27);
            btnS_MID_7.IsEnabled = false;
            update();
        }

        private void btnS_MID_8_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(28);
            btnS_MID_8.IsEnabled = false;
            update();
        }

        private void btnS_MAX_1_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(31);
            btnS_MAX_1.IsEnabled = false;
            update();
        }

        private void btnS_MAX_2_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(32);
            btnS_MAX_2.IsEnabled = false;
            update();
        }

        private void btnS_MAX_3_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(33);
            btnS_MAX_3.IsEnabled = false;
            update();
        }

        private void btnS_MAX_4_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(34);
            btnS_MAX_4.IsEnabled = false;
            update();
        }

        private void btnS_MAX_5_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(35);
            btnS_MAX_5.IsEnabled = false;
            update();
        }

        private void btnS_MAX_6_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(36);
            btnS_MAX_6.IsEnabled = false;
            update();
        }

        private void btnS_MAX_7_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(37);
            btnS_MAX_7.IsEnabled = false;
            update();
        }

        private void btnS_MAX_8_Click(object sender, RoutedEventArgs e)
        {
            seq.Add(38);
            btnS_MAX_8.IsEnabled = false;
            update();
        }
        #endregion
    }
}