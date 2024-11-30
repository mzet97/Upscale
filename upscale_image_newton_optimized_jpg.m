function upscale_image_newton_optimized_jpg(input_image, scale_factor)
    % Carregar a imagem em JPG
    original_image = imread(input_image); % Suporte nativo para JPG no Octave
    original_image = double(original_image); % Converter para double para cálculos

    % Verificar se a imagem é colorida ou em escala de cinza
    if ndims(original_image) == 3
        % Imagem colorida (RGB), processar cada canal separadamente
        red_channel = original_image(:, :, 1);
        green_channel = original_image(:, :, 2);
        blue_channel = original_image(:, :, 3);

        % Aplicar o algoritmo a cada canal
        upscaled_red = process_channel(red_channel, scale_factor);
        upscaled_green = process_channel(green_channel, scale_factor);
        upscaled_blue = process_channel(blue_channel, scale_factor);

        % Combinar os canais novamente
        upscaled_image = cat(3, upscaled_red, upscaled_green, upscaled_blue);
    else
        % Imagem em escala de cinza
        upscaled_image = process_channel(original_image, scale_factor);
    end

    % Salvar a imagem final em JPG
    imwrite(uint8(upscaled_image), 'upscaled_image_newton_optimized.jpg', 'Quality', 95);
    imshow(uint8(upscaled_image));
end

function upscaled_channel = process_channel(channel, scale_factor)
    % Função para processar um único canal de cor ou escala de cinza
    [rows, cols] = size(channel);

    % Calcular as novas dimensões
    new_rows = round(rows * scale_factor);
    new_cols = round(cols * scale_factor);

    % Coordenadas dos pixels originais
    x = 1:rows;
    y = 1:cols;

    % Coordenadas dos novos pixels
    xi = linspace(1, rows, new_rows);
    yi = linspace(1, cols, new_cols);

    % Inicializar o canal interpolado
    upscaled_channel = zeros(new_rows, new_cols);

    % Interpolar usando apenas 4 vizinhos mais próximos
    for i = 1:new_rows
        for j = 1:new_cols
            % Encontrar vizinhos locais para linhas
            row_idx = max(1, floor(xi(i)) - 1):min(rows, ceil(xi(i)) + 1);
            col_idx = max(1, floor(yi(j)) - 1):min(cols, ceil(yi(j)) + 1);

            % Interpolar em duas etapas (primeiro nas linhas, depois nas colunas)
            local_values = channel(row_idx, col_idx);
            local_x = x(row_idx);
            local_y = y(col_idx);

            % Garantir que há pontos suficientes para interpolar
            if length(local_x) < 2 || length(local_y) < 2
                % Se não houver pontos suficientes, use o valor do pixel mais próximo
                upscaled_channel(i, j) = channel(min(rows, round(xi(i))), min(cols, round(yi(j))));
            else
                % Interpolação em duas dimensões
                temp_values = zeros(1, length(local_y));
                for k = 1:length(local_y)
                    % Interpolar ao longo de x para cada y
                    coef_x = local_newton_coefficients(local_x, local_values(:, k)');
                    temp_values(k) = newton_interpolation(local_x, coef_x, xi(i));
                end

                % Agora interpolar esses valores ao longo de y
                coef_y = local_newton_coefficients(local_y, temp_values);
                upscaled_channel(i, j) = newton_interpolation(local_y, coef_y, yi(j));
            end
        end
    end
end

% Função para calcular coeficientes de Newton (apenas 4 pontos locais)
function coef = local_newton_coefficients(xd, yd)
    n = length(xd);
    if n < 2
        % Se houver menos de dois pontos, retorne o valor diretamente
        coef = yd(1); % Retorna o único valor disponível
        return;
    end
    coef = yd;
    for j = 2:n
        coef(j:n) = (coef(j:n) - coef(j-1:n-1)) ./ (xd(j:n) - xd(1:n-j+1));
    end
end

% Função para avaliar o polinômio de Newton
function result = newton_interpolation(xd, coef, x)
    n = length(coef);
    result = coef(n);
    for j = n-1:-1:1
        result = result .* (x - xd(j)) + coef(j);
    end
end

