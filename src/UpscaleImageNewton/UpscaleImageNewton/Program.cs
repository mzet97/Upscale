// See https://aka.ms/new-console-template for more information

var options = 5;

Console.WriteLine("Bem-vindo ao UpscaleImageNewton!");
Console.WriteLine("Digite o caminho da imagem que deseja redimensionar:");
var path = Console.ReadLine();
if (path == null)
{
    Console.WriteLine("Caminho inválido.");
    return;
}
Console.WriteLine("Digite o caminho de saída da imagem:");
var pathOut = Console.ReadLine();
if (pathOut == null)
{
    Console.WriteLine("Caminho inválido.");
    return;
}
Console.WriteLine("Digite o fator de redimensionamento:");
var factor = Convert.ToDouble(Console.ReadLine());
if (factor <= 0)
{
    Console.WriteLine("Fator inválido.");
    return;
}

Console.WriteLine("Start:" + DateTime.Now.ToString());
ImageUpscalerParallel.UpscaleImageNewtonOptimizedJpg(path, pathOut, factor);

decimal psnr = PSNR.Calculate(path, pathOut);

if (psnr > 30)
{
    Console.WriteLine("PSNR: " + psnr);
    Console.WriteLine("Imagem redimensionada com sucesso!");
}
else if (psnr > 20)
{
    Console.WriteLine("PSNR: " + psnr);
    Console.WriteLine("Imagem redimensionada com sucesso, mas a qualidade da imagem é aceitável.");
}
else if (psnr < 20)
{
    Console.WriteLine("PSNR: " + psnr);
    Console.WriteLine("Imagem redimensionada com sucesso, mas a qualidade da imagem é baixa.");
}
Console.WriteLine("End:" + DateTime.Now.ToString());