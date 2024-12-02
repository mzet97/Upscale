function psnr = calculate_psnr(image_original_path, image_altered_path)

    I_original = imread(image_original_path);
    I_altered = imread(image_altered_path);

    if size(I_original, 3) ~= size(I_altered, 3)
        error("As imagens devem ter o mesmo número de canais (grayscale ou RGB).");
    end

    if size(I_original, 1) ~= size(I_altered, 1) || size(I_original, 2) ~= size(I_altered, 2)
        I_altered = imresize(I_altered, [size(I_original, 1), size(I_original, 2)]);
    end

    I_original = double(I_original);
    I_altered = double(I_altered);

    mse = mean((I_original(:) - I_altered(:)).^2);

    if mse == 0
        psnr = Inf;
        return;
    end

    max_pixel = 255.0;

    psnr = 10 * log10((max_pixel^2) / mse);
end

image_original_path = 'original.jpg';
image_altered_path = 'altered.jpg';

psnr_value = calculate_psnr(image_original_path, image_altered_path);
fprintf('PSNR: %.2f dB\n', psnr_value);
