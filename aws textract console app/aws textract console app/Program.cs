using Amazon.Textract;
using Amazon.Textract.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace CSharpAWSTextract
{
    class Program
    {
        static void Main(string[] args)
        {

            const string LocalEmploymentFile = "C:\\Users\\Acer PC\\Pictures\\invoice.png";

            TextExtractDemo.Get_kv_map(LocalEmploymentFile);
            Console.ReadLine();

        }

    }
    public class TextExtractDemo
    {
        public static void Get_kv_map(string LocalEmploymentFile)
        {
            var readFile = File.ReadAllBytes(LocalEmploymentFile);

            MemoryStream stream = new MemoryStream(readFile);
            AmazonTextractClient abcdclient = new AmazonTextractClient();

            AnalyzeDocumentRequest analyzeDocumentRequest = new AnalyzeDocumentRequest
            {
                Document = new Document
                {
                    Bytes = stream

                },
                FeatureTypes = new List<string>
                {
                    FeatureType.FORMS
                }


            };
            var analyzeDocumentResponse =  abcdclient.AnalyzeDocument(analyzeDocumentRequest);
            Console.WriteLine(analyzeDocumentResponse.);
            //Get the text blocks
            List<Block> blocks = analyzeDocumentResponse.Blocks;

            //get key and value maps
            List<Block> key_map = new List<Block>();
            List<Block> value_map = new List<Block>();
            List<Block> block_map = new List<Block>();
            foreach (Block block in blocks)
            {
                var block_id = block.Id;
                block_map.Add(block);
                if (block.BlockType == BlockType.KEY_VALUE_SET)
                {
                    if (block.EntityTypes.Contains("KEY"))
                    {
                        key_map.Add(block);
                    }
                    else
                    {
                        value_map.Add(block);
                    }

                }

            }

            //Get Key Value relationship
            var getKeyValueRelationship = Get_kv_relationship(key_map, value_map, block_map);

            foreach (KeyValuePair<string, string> kvp in getKeyValueRelationship)
            {
                Console.WriteLine(" {0} : {1}", kvp.Key, kvp.Value);
            }
            //for table
            //var gettable = get_rows_columns_map(block_map);


        }
        public static Dictionary<string, string> Get_kv_relationship(List<Block> key_map, List<Block> value_map, List<Block> block_map)
        {
            List<string> kvs1 = new List<string>();
            Dictionary<string, string> kvs = new Dictionary<string, string>();
            Block value_block = new Block();
            string key, val = string.Empty;
            foreach (var block in key_map)
            {
                value_block = Find_value_block(block, value_map);
                key = Get_text(block, block_map);
                val = Get_text(value_block, block_map);
                kvs.Add(key, val);
            }

            return kvs;

        }

        public static Block Find_value_block(Block block, List<Block> value_map)
        {
            Block value_block = new Block();
            foreach (var relationship in block.Relationships)
            {
                if (relationship.Type == "VALUE")
                {
                    foreach (var value_id in relationship.Ids)
                    {
                        value_block = value_map.First(x => x.Id == value_id);
                    }

                }

            }
            return value_block;

        }

        public static string Get_text(Block result, List<Block> block_map)
        {
            string text = string.Empty;
            Block word = new Block();

            if (result.Relationships.Count > 0)
            {
                foreach (var relationship in result.Relationships)
                {
                    if (relationship.Type == "CHILD")
                    {
                        foreach (var child_id in relationship.Ids)
                        {
                            word = block_map.First(x => x.Id == child_id);
                            if (word.BlockType == "WORD")
                            {
                                text += word.Text + " ";
                            }
                            if (word.BlockType == "SELECTION_ELEMENT")
                            {
                                if (word.SelectionStatus == "SELECTED")
                                {
                                    text += "X ";
                                }

                            }
                        }
                    }
                }
            }
            return text;

        }





        public static Dictionary<string, string> get_rows_columns_map(Block result,string table_result, List<Block> block_map)
        {
            int row_index;
            int col_index;
            Dictionary<string, string> newlist = new Dictionary<string, string>();
            foreach (var relationship in result.Relationships)
            {
                string text = string.Empty;
                Block cell = new Block();
                if (relationship.Type == "CHILD")
                {
                    foreach (var child_id in relationship.Ids)
                    {
                        cell = block_map.First(x => x.Id == child_id);
                        if (cell.BlockType == "CELL")
                        {
                            row_index = cell.RowIndex;
                            col_index = cell.ColumnIndex;
                            if (row_index in newlist)
                            {

                            }
                        }
                    }
                }
            }
        }
    }
}