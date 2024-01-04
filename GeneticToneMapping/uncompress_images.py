import os
import OpenEXR

def uncompress_images(compressed_path, uncompressed_path):
    for filename in os.listdir(compressed_path):
        full_compressed_path = os.path.join(compressed_path, filename)
        full_uncompressed_path = os.path.join(uncompressed_path, filename)
        os.system('convert "{}" -compress none "{}"'.format(full_compressed_path, full_uncompressed_path))

def get_exr_resolution(exr_file):
    exr_input = OpenEXR.InputFile(exr_file)

    header = exr_input.header()
    display_window = header['displayWindow']

    width = display_window.max.x - display_window.min.x + 1
    height = display_window.max.y - display_window.min.y + 1

    return width, height

def resize_images(uncompressed_path):
    for filename in os.listdir(uncompressed_path):
        full_uncompressed_path = os.path.join(uncompressed_path, filename)
        width, height = get_exr_resolution(full_uncompressed_path)
        percentage = 100
        while width >= 1500:
            percentage = percentage // 2
            width = width // 2
            height = height // 2

        if percentage != 100:
            os.system('convert "{}" -resize {}% "{}"'.format(full_uncompressed_path, percentage, full_uncompressed_path))

def main():
    compressed_path = "./Compressed/"
    uncompressed_path = "./Uncompressed/"

    #uncompress_images(compressed_path, uncompressed_path)
    resize_images(uncompressed_path)

if __name__ == "__main__":
    main()