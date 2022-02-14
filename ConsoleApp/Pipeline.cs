using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGeneratorLib;

namespace ConsoleApp
{
    public class Pipeline
    {
         
        
         public static string resultPath;
   

        public void Start(List<string> pathes, string resPath)
        {
            List<PipeItem> items = new List<PipeItem>();
            resultPath = resPath;
            
            for (int i = 0; i < pathes.Count; i++)
            {
                items.Add(new PipeItem(pathes[i])); 
            }
            
            List<ActionBlock<PipeItem>> writes = new List<ActionBlock<PipeItem>>();

            var read = new TransformBlock<PipeItem, PipeItem>( 
                async item =>
                {
                    Console.WriteLine("Reading");
                    return await Task<PipeItem>.Factory.StartNew(() => new PipeItem(item.path,File.ReadAllText(item.path)));
                }
            );
            var generate = new TransformBlock<PipeItem, PipeItem>(
                async item =>
                {
                    ITestGenerator Generator = new TestGenerator(); 
                    Console.WriteLine("Generating"); 
                    return await Task<PipeItem>.Factory.StartNew(() => new PipeItem(item.path, Generator.Generate(item.code).Result));
                });
            
            var write = new ActionBlock<PipeItem>(
                async item =>
                {
                    Console.WriteLine("Writing"); 
                   
                        string name = Path.GetFileNameWithoutExtension(item.path); 
                        name = name.Split('\\').Last(); 
                        File.WriteAllTextAsync(resultPath + name + "Result" + Path.GetExtension(item.path), item.code);
                    
                });
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            read.LinkTo(generate, linkOptions);
            generate.LinkTo(write, linkOptions);
            
            foreach (PipeItem item in items)
            {
                read.Post(item);
                writes.Add(write);
            }
            read.Complete();
            foreach (ActionBlock<PipeItem> action in writes)
            {
                action.Completion.Wait();
            }
            Console.Write("End");
        }
        
        
    }
}