function upscale_image_newton_optimized_jpg(input_image, scale_factor)

    original_image = imread(input_image);
    original_image = double(original_image);

    if ndims(original_image) == 3

        red_channel = original_image(:, :, 1);
        green_channel = original_image(:, :, 2);
        blue_channel = original_image(:, :, 3);

        upscaled_red = process_channel(red_channel, scale_factor);
        upscaled_green = process_channel(green_channel, scale_factor);
        upscaled_blue = process_channel(blue_channel, scale_factor);

        upscaled_image = cat(3, upscaled_red, upscaled_green, upscaled_blue);
    else
        upscaled_image = process_channel(original_image, scale_factor);
    end

    imwrite(uint8(upscaled_image), 'upscaled_image_newton_optimized.jpg', 'Quality', 95);
    imshow(uint8(upscaled_image));
end

function upscaled_channel = process_channel(channel, scale_factor)

    [rows, cols] = size(channel);

    new_rows = round(rows * scale_factor);
    new_cols = round(cols * scale_factor);

    x = 1:rows;
    y = 1:cols;

    xi = linspace(1, rows, new_rows);
    yi = linspace(1, cols, new_cols);

    upscaled_channel = zeros(new_rows, new_cols);

    for i = 1:new_rows
        for j = 1:new_cols
            row_idx = max(1, floor(xi(i)) - 1):min(rows, ceil(xi(i)) + 1);
            col_idx = max(1, floor(yi(j)) - 1):min(cols, ceil(yi(j)) + 1);

            local_values = channel(row_idx, col_idx);
            local_x = x(row_idx);
            local_y = y(col_idx);

            if length(local_x) < 2 || length(local_y) < 2
                upscaled_channel(i, j) = channel(min(rows, round(xi(i))), min(cols, round(yi(j))));
            else
                temp_values = zeros(1, length(local_y));
                for k = 1:length(local_y)
                    coef_x = local_newton_coefficients(local_x, local_values(:, k)');
                    temp_values(k) = newton_interpolation(local_x, coef_x, xi(i));
                end

                coef_y = local_newton_coefficients(local_y, temp_values);
                upscaled_channel(i, j) = newton_interpolation(local_y, coef_y, yi(j));
            end
        end
    end
end

function coef = local_newton_coefficients(xd, yd)
    n = length(xd);
    if n < 2
        coef = yd(1);
        return;
    end
    coef = yd;
    for j = 2:n
        coef(j:n) = (coef(j:n) - coef(j-1:n-1)) ./ (xd(j:n) - xd(1:n-j+1));
    end
end

function result = newton_interpolation(xd, coef, x)
    n = length(coef);
    result = coef(n);
    for j = n-1:-1:1
        result = result .* (x - xd(j)) + coef(j);
    end
end

