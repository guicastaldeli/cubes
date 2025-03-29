import * as THREE from 'three';
import { mergeGeometries } from 'three/addons/utils/BufferGeometryUtils.js';

import { scene } from './main.js';

export function createParticles() {
    const geometries = [];
    const particleCount = 10000 * 50;
    const maxHeight = -50;
    
    for(let i = 0; i < particleCount; i++) {
        const geometry = new THREE.BufferGeometry();
        const positions = [];
        const sizes = [];
    
        positions.push(Math.random() * 200 - 100);
        positions.push(maxHeight);
        positions.push(Math.random() * 200 - 100);
    
        sizes.push(Math.random() * 0.5 + 0.1);
    
        geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
        geometry.setAttribute('size', new THREE.Float32BufferAttribute(sizes, 1));
    
        geometries.push(geometry);
    }
    
    const mergedGeometry = mergeGeometries(geometries);

    if(!mergedGeometry.attributes.position) {
        console.error('err');
        return;
    }
    
    const material = new THREE.PointsMaterial({
        color: 'rgb(255, 255, 255)',
        size: 0.1,
        sizeAttenuation: true,
    });
    
    const particle = new THREE.Points(mergedGeometry, material);
    scene.add(particle);

    //Animate
    const fallSpeed = 0.1;
    
    function animateParticles() {
        const positions = particle.geometry.attributes.position.array;
    
        for(let i = 1; i < positions.length; i += 3) {
            positions[i] -= fallSpeed;
    
            if(positions[i] < maxHeight) {
                positions[i] = Math.random() * 100;
                positions[i - 1] = Math.random() * 200 - 100;
                positions[i + 1] = Math.random() * 200 - 100;
            }
        }
    
        particle.geometry.attributes.position.needsUpdate = true;
    }

    return animateParticles;
}