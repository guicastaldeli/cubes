import * as THREE from 'three';
import { mergeGeometries } from 'three/addons/utils/BufferGeometryUtils.js';

import { scene } from './main-scene.js';

export function createMenuCubes() {
    const cubes = [];
    const geometries = [];
    const moveSpeed = 0.01;
    const zMin = -55;
    const zMax = 55;
    
    for(let i = 0; i < 500; i++) {
        //Pos
        const rx = Math.floor(Math.random() * (zMax - zMin) + zMin) / 1.7;
        const ry = Math.floor(Math.random() * (zMax - zMin) + zMin) / 1.5;
        const rz = Math.floor(Math.random() * (zMax - zMin) + zMin);

        const pos = {
            x: rx,
            y: ry,
            z: rz
        }
    
        const scale = {
            width: Math.random() * 2 + 0.5,
            height: Math.random() * 2 + 0.5,
            depth: Math.random() * 2 + 0.5
        }

        const geometry = new THREE.BoxGeometry(scale.width, scale.height, scale.depth);
        geometries.push(geometry);

        const randomColor = new THREE.Color();
        randomColor.setHSL(Math.random() * 360, 0.5 + Math.random() * 0.3, 0.3 + Math.random() * 0.2);

        const material = new THREE.MeshStandardMaterial({ color: randomColor, transparent: true, opacity: 1, fog: true });
        const cube = new THREE.Mesh(geometry, material);

        material.depthWrite = true;
        material.depthTest = true;

        cube.receiveShadow = true;
    
        cube.position.x = pos.x;
        cube.position.y = pos.y;
        cube.position.z = pos.z;

        cubes.push(cube);
    }
    
    //Merged Cubes
        const mergedGeometry = mergeGeometries(geometries);
        mergedGeometry.computeBoundingBox();
        mergedGeometry.computeBoundingSphere();
        
        //Materal
            const colors = new Float32Array(mergedGeometry.attributes.position.count * 3);

            for(let i = 0; i < colors.length; i += 3) {
                const randomColor = new THREE.Color();
                randomColor.setHSL(Math.random() * 0.5 + 1, 0.2 + Math.random() * 0.3, 0.3 + Math.random() * 0.2);

                colors[i] = randomColor.r;
                colors[i + 2] = randomColor.g;
                colors[i + 3] = randomColor.b;
            }

            mergedGeometry.setAttribute('color', new THREE.BufferAttribute(colors, 3));

            const mergedMaterial = new THREE.MeshStandardMaterial({ vertexColors: true, transparent: true, opacity: 1, fog: true });
        //

        const mergedCubes = new THREE.Mesh(mergedGeometry, mergedMaterial);
        mergedCubes.visible = false;

        scene.add(mergedCubes);
    //

    function updateCubesMov() {
        cubes.forEach(cube => {
            cube.position.z += moveSpeed;

            if(cube.position.z > zMax) {
                cube.position.z = zMin;
            } else if(cube.position.z < zMin) {
                cube.position.z = zMax;
            }
        });
    }

    function animateCubes() {
        updateCubesMov();
        requestAnimationFrame(animateCubes);
    }

    animateCubes();

    return cubes;
}