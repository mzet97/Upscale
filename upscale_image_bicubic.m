function upscale_image_bicubic(input_image, scale_factor)

    original_image = imread(input_image);
    original_image = double(original_image);

    [rows, cols, num_channels] = size(original_image);

    new_rows = round(rows * scale_factor);
    new_cols = round(cols * scale_factor);

    [X, Y] = meshgrid(1:cols, 1:rows);

    [Xq, Yq] = meshgrid(linspace(1, cols, new_cols), linspace(1, rows, new_rows));

    upscaled_image = zeros(new_rows, new_cols, num_channels);

    for c = 1:num_channels
        channel = original_image(:, :, c);

        upscaled_image(:, :, c) = bicubic_interpolation(channel, X, Y, Xq, Yq);
    end

    upscaled_image = uint8(min(max(upscaled_image, 0), 255));

    imwrite(upscaled_image, 'upscaled_image_bicubic.jpg', 'Quality', 95);
    imshow(upscaled_image);
end

function interp_channel = bicubic_interpolation(channel, X, Y, Xq, Yq)
    interp_channel = interp2(X, Y, channel, Xq, Yq, 'cubic');
end
