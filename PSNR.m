function psnr = calculate_psnr(image_original_path, image_altered_path)
    % Carregar as imagens
    I_original = imread(image_original_path);
    I_altered = imread(image_altered_path);

    % Garantir que ambas sejam em escala de cinza ou RGB
    if size(I_original, 3) ~= size(I_altered, 3)
        error("As imagens devem ter o mesmo número de canais (grayscale ou RGB).");
    end

    % Verificar resoluções e redimensionar se necessário
    if size(I_original, 1) ~= size(I_altered, 1) || size(I_original, 2) ~= size(I_altered, 2)
        I_altered = imresize(I_altered, [size(I_original, 1), size(I_original, 2)]);
    end

    % Converter as imagens para double para cálculos precisos
    I_original = double(I_original);
    I_altered = double(I_altered);

    % Calcular o erro quadrático médio (MSE)
    mse = mean((I_original(:) - I_altered(:)).^2);

    % Se o MSE for zero, as imagens são idênticas
    if mse == 0
        psnr = Inf; % PSNR infinito para imagens idênticas
        return;
    end

    % Valor máximo de pixel (255 para imagens de 8 bits)
    max_pixel = 255.0;

    % Calcular o PSNR
    psnr = 10 * log10((max_pixel^2) / mse);
end

% Exemplo de uso:
image_original_path = 'original.jpg'; % Substitua pelo caminho real
image_altered_path = 'altered.jpg';   % Substitua pelo caminho real

psnr_value = calculate_psnr(image_original_path, image_altered_path);
fprintf('PSNR: %.2f dB\n', psnr_value);
