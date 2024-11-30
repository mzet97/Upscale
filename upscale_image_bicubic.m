function upscale_image_bicubic(input_image, scale_factor)
    % Carregar a imagem
    original_image = imread(input_image);
    original_image = double(original_image);

    % Obter dimensões da imagem original
    [rows, cols, num_channels] = size(original_image);

    % Calcular as novas dimensões
    new_rows = round(rows * scale_factor);
    new_cols = round(cols * scale_factor);

    % Coordenadas dos pixels originais
    [X, Y] = meshgrid(1:cols, 1:rows);

    % Coordenadas dos novos pixels
    [Xq, Yq] = meshgrid(linspace(1, cols, new_cols), linspace(1, rows, new_rows));

    % Inicializar a imagem ampliada
    upscaled_image = zeros(new_rows, new_cols, num_channels);

    % Interpolar cada canal
    for c = 1:num_channels
        % Extrair o canal atual
        channel = original_image(:, :, c);

        % Aplicar a interpolação bicúbica
        upscaled_image(:, :, c) = bicubic_interpolation(channel, X, Y, Xq, Yq);
    end

    % Converter para uint8 e garantir que os valores estejam no intervalo [0, 255]
    upscaled_image = uint8(min(max(upscaled_image, 0), 255));

    % Salvar e exibir a imagem ampliada
    imwrite(upscaled_image, 'upscaled_image_bicubic.jpg', 'Quality', 95);
    imshow(upscaled_image);
end

function interp_channel = bicubic_interpolation(channel, X, Y, Xq, Yq)
    % Função para realizar a interpolação bicúbica em um único canal
    interp_channel = interp2(X, Y, channel, Xq, Yq, 'cubic');
end
