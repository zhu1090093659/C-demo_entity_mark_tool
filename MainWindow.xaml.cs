using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public enum Relation
    {
        POSITIVE,NEGATIVE,NEUTRAL
    }

    public partial class MainWindow : Window
    {
        private int curOrgId = 0;
        private int dataCount;
        private List<MyData> myDatas;   // 整个文件的数据
        private List<ShowData> myDataList;  //一条数据
        private ShowData curRow = new ShowData();
        private List<string> targetList = new List<string>();
        private List<string> opinionList = new List<string>();
        private List<string> relationList = new List<string>();

        public int DataGridIndex { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

        }
        private void MarkTokenOutput(string color, RichTextBox richTextBox1, int selectLength, TextPointer tpStart, TextPointer tpOffset)
        {
            TextRange range = richTextBox1.Selection;
            range.Select(tpStart, tpOffset);

            // 高亮选择
            if (color == "blue")
            {
                range.ApplyPropertyValue(TextElement.ForegroundProperty,
                new SolidColorBrush(Colors.Blue));
                range.ApplyPropertyValue(TextElement.FontWeightProperty,
                FontWeights.Bold);
            }
            else
            {
                range.ApplyPropertyValue(TextElement.ForegroundProperty,
                new SolidColorBrush(Colors.Red));
                range.ApplyPropertyValue(TextElement.FontWeightProperty,
                FontWeights.Bold);
            }
            TextPointer position = richTextBox1.Document.ContentStart;
            TextPointer start = position.GetPositionAtOffset(2);
            TextPointer end = richTextBox1.Document.ContentEnd;
            TextRange range1 = richTextBox1.Selection;
            range1.Select(end, end);
            range1.ApplyPropertyValue(TextElement.ForegroundProperty,
            new SolidColorBrush(Colors.Black));
            
        }


        private void tokenShow(MyData data)
        {
            //RichTextBox tokenOutput1 = tokenOutput;
            //tokenOutput1.Text = File.ReadAllText(openFileDialog.FileName);
            // 处理token数据，将List<string> 转为string
            string ?t="";
            foreach (var item in data.tokens)
            {
                t += item;
            }
            FlowDocument myFlowDoc = new FlowDocument();

            // Create a Run of plain text and some bold text.
            Run myRun = new Run(t);

            // Create a paragraph and add the Run and Bold to it.
            Paragraph myParagraph = new Paragraph();
            myParagraph.Inlines.Add(myRun);

            // Add the paragraph to the FlowDocument.
            myFlowDoc.Blocks.Add(myParagraph);


            // Add initial content to the RichTextBox.
            tokenOutput.Document = myFlowDoc;

        }


        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //过滤文件
            openFileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                myDatas =  JsonHelper.DeserializeJsonToList<MyData>(File.ReadAllText(openFileDialog.FileName));
                dataCount = myDatas.Count;
                //curOrgId = myDatas[0].orig_id;
                // 展示第一条token数据
                tokenShow(myDatas[0]);
                // 加载数据
                myDataList = loadShowData();
                // 展示数据
                ShowDataGrid(sender, e);
                WriteToLog("打开文件：" + openFileDialog.FileName);
                
            }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            
        }

        private void EntityClick(object sender, RoutedEventArgs e)
        {
            // 实体标注
            // 添加实体数据
            curRow.Target = tokenOutput.Selection.Text;
            MarkTokenOutput("red", tokenOutput, curRow.Target.Length,
                tokenOutput.Selection.Start, tokenOutput.Selection.End);

            targetList.Add(curRow.Target);
            if (curRow.Relation == null &&
                curRow.Opinion == null)
                myDataList.Add(curRow);
            else
                myDataList[myDataList.Count - 1].Target = curRow.Target;
            // 展示数据
            ShowDataGrid(sender, e);
            WriteToLog($"标记实体 {curRow.Target}");

        }

        private void OpinionClick(object sender, RoutedEventArgs e)
        {
            // 情感实体标注
            // 情感实体数据
            curRow.Opinion = tokenOutput.Selection.Text;
            MarkTokenOutput("blue", tokenOutput, curRow.Opinion.Length,
                tokenOutput.Selection.Start, tokenOutput.Selection.End);
            opinionList.Add(curRow.Opinion);
            if (curRow.Relation == null &&
                curRow.Target == null)
                myDataList.Add(curRow);
            else
                myDataList[myDataList.Count - 1].Opinion = curRow.Opinion;
            // 展示数据
            ShowDataGrid(sender, e);
            WriteToLog($"标记情感实体 {curRow.Opinion}");
        }

        private void OnDataComplete(object sender, DataGridRowEditEndingEventArgs e)
        {

            DataGridCellInfo cell_Info = dataGrid1.SelectedCells[0];
            if(cell_Info.Column.DisplayIndex == 2)
            {
                ShowData data =  cell_Info.Item as ShowData;
                curRow.Relation = data.Relation;
                if (curRow.Opinion == null &&
                    curRow.Target == null)
                    myDataList.Add(curRow);
                else
                    myDataList[myDataList.Count - 1].Opinion = curRow.Opinion;
                // 展示数据
                ShowDataGrid(sender, new RoutedEventArgs());
                WriteToLog($"标记关系 {curRow.Relation}");
            }

        }

        private List<ShowData> loadShowData()
        {
            // 加载数据源
            MyData temp = myDatas[curOrgId];
            List<ShowData> myShowData = new List<ShowData>();

            List<string> tokens = new List<string>();

            // 清空target列表和opinion列表
            if(targetList!=null)
            {
                targetList.Clear();
            }
            if(opinionList != null)
                opinionList.Clear();
            if(relationList != null)
                relationList.Clear();
            foreach (var item in temp.relations)
            {
                ShowData row = new ShowData();
                List<EntitiesItem> entities = temp.entities;
                EntitiesItem target = entities[item.head];
                EntitiesItem opinion = entities[item.tail];
                for (int i = 0; i < entities.Count; i++)
                    row.tokens += temp.tokens[i];
                for (int i = target.start; i < target.end; i++)
                    row.Target += temp.tokens[i];
                for (int i = opinion.start; i < opinion.end; i++)
                    row.Opinion += temp.tokens[i];


                row.Relation = item.type;
                targetList.Add(row.Target);
                relationList.Add(row.Relation);
                opinionList.Add(row.Opinion);
                myShowData.Add(row);
            }
            return myShowData;
        }

        private void ShowDataGrid(object sender, RoutedEventArgs e)
        {
            //BindingOperations.ClearBinding(dataGrid1,DataGrid.ItemsSourceProperty);
            if(dataGrid1 != null)
                dataGrid1.ItemsSource = null;

            dataGrid1.ItemsSource = myDataList;
        }


        private void PreOrgData_Click(object sender, RoutedEventArgs e)
        {
            if (curOrgId - 1 >= 0)
            {
                curOrgId--;
                tokenShow(myDatas[curOrgId]);
                myDataList = loadShowData();
                ShowDataGrid(sender, e);
                curRow = new ShowData();
                WriteToLog("返回上一条数据");
            }
            else
            {
                MessageBox.Show("已经是第一条数据了", "ERROR");
            }
        }

        private void NextOrgData_Click(object sender, RoutedEventArgs e)
        {
            if (curOrgId + 1 < myDatas.Count)
            {
                curOrgId++;
                tokenShow(myDatas[curOrgId]);
                myDataList = loadShowData();
                ShowDataGrid(sender, e);
                curRow = new ShowData();
                WriteToLog("跳到下一条数据");
            }
            else
            {
                MessageBox.Show("已经是最后一条数据了", "ERROR");
            }
        }

        private void ChangeOrgData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int t = Int32.Parse(OrgDataId.Text);
                if(t < 0 || t >= myDatas.Count)
                {
                    MessageBox.Show($"{t} 不是一个有效org_id", "ERROR");
                }
                else
                {
                    curOrgId = t;
                    tokenShow(myDatas[curOrgId]);
                    myDataList = loadShowData();
                    ShowDataGrid(sender, e);
                    WriteToLog($"跳转到第 {t} 条数据");
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("请输入数字", "ERROR");
            }
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            if(curRow.Target!=null && curRow.Relation!=null && curRow.Opinion != null)
            {
                // 清空行数据
                curRow = new ShowData();
                WriteToLog("保存一对实体-情感-关系数据");
            }
            else
            {
                if (curRow.Opinion == null)
                {
                    MessageBox.Show($"Opinion 不能为空", "ERROR");
                }
                if (curRow.Target == null)
                {
                    MessageBox.Show($"Target 不能为空", "ERROR");
                }
                if (curRow.Relation == null)
                {
                    MessageBox.Show($"Relation 不能为空", "ERROR");
                }
            }
        }

        private void SaveAllItem_Click(object sender, RoutedEventArgs e)
        {
            // 清除原数据
            myDatas[curOrgId].entities.Clear();
            myDatas[curOrgId].relations.Clear();
            foreach (var item in myDataList)
            {
                EntitiesItem entitiesItem = new EntitiesItem();
                EntitiesItem entitiesItem2 = new EntitiesItem();
                entitiesItem.type = "target";
                // 对于实体位置的标注这里还需要改进
                entitiesItem.start = item.tokens.IndexOf(item.Target);
                entitiesItem.end = entitiesItem.start+ item.Target.Length;
                entitiesItem2.type = "opinion";
                entitiesItem2.start = item.tokens.IndexOf(item.Opinion);
                entitiesItem2.end = entitiesItem2.start + item.Opinion.Length;
                myDatas[curOrgId].entities.Add(entitiesItem);
                myDatas[curOrgId].entities.Add(entitiesItem2);
                RelationsItem relationsItem = new RelationsItem();
                relationsItem.type = item.Relation;
                relationsItem.head = myDatas[curOrgId].entities.Count - 2;
                relationsItem.tail = relationsItem.head + 1;
                myDatas[curOrgId].relations.Add(relationsItem);
                WriteToLog("保存一条标注数据");

            }
        }


        public void WriteToLog(string message)
        {
            
            string strTime = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] ";
            logTextBox.AppendText(strTime + message + "\r");

            //if (logTextBox.ExtentHeight > 200)
            //{
            //    logTextBox.Document.Blocks.Clear();
            //}
        }


    }

    
}
