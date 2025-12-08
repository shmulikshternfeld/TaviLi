import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class ImageCompressionService {

    async compressImage(file: File, maxWidth: number = 500, quality: number = 0.7): Promise<File> {
        const format = 'image/jpeg'; // Force JPEG for standard compression
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsDataURL(file);
            reader.onload = (event: any) => {
                const img = new Image();
                img.src = event.target.result;
                img.onload = () => {
                    const elem = document.createElement('canvas');
                    let width = img.width;
                    let height = img.height;

                    // Resize logic
                    if (width > maxWidth) {
                        height = Math.round(height * (maxWidth / width));
                        width = maxWidth;
                    }

                    elem.width = width;
                    elem.height = height;

                    const ctx = elem.getContext('2d');
                    if (!ctx) {
                        reject(new Error('Could not get canvas context'));
                        return;
                    }

                    ctx.drawImage(img, 0, 0, width, height);

                    elem.toBlob((blob) => {
                        if (!blob) {
                            reject(new Error('Compression failed'));
                            return;
                        }
                        // Create new file
                        const newFileName = file.name.replace(/\.[^/.]+$/, "") + ".jpg";
                        const newFile = new File([blob], newFileName, {
                            type: format,
                            lastModified: Date.now(),
                        });
                        resolve(newFile);
                    }, format, quality);
                };
                img.onerror = error => reject(error);
            };
            reader.onerror = error => reject(error);
        });
    }
}
