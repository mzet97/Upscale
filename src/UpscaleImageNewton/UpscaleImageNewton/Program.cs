// See https://aka.ms/new-console-template for more information
Console.WriteLine("Start:" + DateTime.Now.ToString());
ImageUpscalerParallelGPU.UpscaleImageNewtonOptimizedJpg(@"E:\TI\Projetos\octave\upscale\imagem_original.jpg", 50);
Console.WriteLine("End:" + DateTime.Now.ToString());