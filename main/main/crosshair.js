import * as THREE from 'three';

export function createCrosshair(camera) {
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    const size = 256;

    canvas.width = size;
    canvas.height = size;

    ctx.strokeStyle = 'rgb(255, 255, 255)';
    ctx.lineWidth = 8;

    ctx.beginPath();
    ctx.moveTo(size / 4, size / 2);
    ctx.lineTo((3 * size) / 4, size / 2);
    ctx.stroke();

    const texture = new THREE.CanvasTexture(canvas);
    texture.needsUpdate = true;
    texture.minFilter = THREE.NearestFilter;
    texture.magFilter = THREE.NearestFilter;
    texture.generateMipmaps = false;

    const material = new THREE.SpriteMaterial({ map: texture, depthTest: false, sizeAttenuation: false });
    const crosshair = new THREE.Sprite(material);

    crosshair.scale.set(0.05, 1, 1);
    crosshair.position.set(0, 0, -0.1);

    camera.add(crosshair);
}