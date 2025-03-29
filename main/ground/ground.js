import * as THREE from 'three';
import * as CANNON from 'https://cdn.jsdelivr.net/npm/cannon-es/dist/cannon-es.js';
import { mergeGeometries } from 'three/addons/utils/BufferGeometryUtils.js';

//Textures
    const testText = new THREE.TextureLoader().load('../../assets/textures/ground-texture-max.png');
//

export function createGround(world) {
    const cubes = new THREE.Group();
    const space = 1;
    const rows = 35;

    //Cols
    const minCols = 15;
    const maxCols = 30;
    const cols = Math.floor(Math.random() * (maxCols - minCols) + minCols);
    
    //Layers
    const minLayers = 15;
    const maxLayers = 30;
    const layers = Math.floor(Math.random() * (maxLayers - minLayers) + minLayers);

    const pos = {
        x: -5,
        y: 0,
        z: 3,
    }

    const scale = {
        width: 1,
        height: 1,
        depth: 1,
    }

    for(let layer = 0; layer < layers; layer++) {
        const ground = new THREE.Group();
        const geometries = [];

        for(let row = 0; row < rows; row++) {
            for(let col = 0; col < cols; col++) {
                const geometry = new THREE.BoxGeometry(scale.width, scale.height, scale.depth);
                geometry.translate(
                    col * space,
                    -row,
                    layer
                )
                geometries.push(geometry);

            }
        }

        const mergedGeometry = mergeGeometries(geometries);
        const material = new THREE.MeshStandardMaterial({ map: testText });
        const cube = new THREE.Mesh(mergedGeometry, material);

        cube.receiveShadow = true;

        ground.add(cube);

        ground.position.x = pos.x;
        ground.position.y = pos.y;
        ground.position.z = pos.z;

        cubes.add(ground);
    }

    const groundLimits = {
        minX: pos.x - 0.65, //Left
        maxX: pos.x + (cols * space) - 0.65, //Top
        minZ: pos.z - 0.65, //Right
        maxZ: pos.z + layers - 0.65 //Bottom
    }

    const center = {
        x: pos.x + (cols * space) / 2,
        y: pos.y + 1.5,
        z: pos.z + (layers / 2)
    }

    //Collision
    const groundMaterial = new CANNON.Material('ground');
    const playerMaterial = new CANNON.Material('player');

    const contactMaterial = new CANNON.ContactMaterial(
        groundMaterial, playerMaterial,
        new CANNON.Material(),
        {
            friction: 0.1,
            restitution: 0.1
        }
    );

    const GROUND_GROUP = 1;
    const PLAYER_GROUP = 2;

    const groundColSize = new CANNON.Vec3(
        (cols * space) * 0.5, 
        rows * 0.5, 
        (layers) * 0.5
    );

    const groundBody = new CANNON.Body({
        mass: 0,
        position: new CANNON.Vec3(
            pos.x + (cols * space) / 2 - 0.5,
            pos.y - 17,
            pos.z + (layers / 2) - 0.5
        ),
        shape: new CANNON.Box(groundColSize),
        material: groundMaterial,
        type: CANNON.Body.STATIC,
    });

    //Box Helper
        const geometry = new THREE.BoxGeometry(groundColSize.x * 2, groundColSize.y, groundColSize.z * 2);
        const material = new THREE.MeshBasicMaterial({ color: 0x00f00, wireframe: true });
        const cube = new THREE.Mesh(geometry, material);

        cube.position.set(groundBody.position.x, groundBody.position.y, groundBody.position.z);

        const boxHelper = new THREE.BoxHelper(cube, 0xffff00);
        boxHelper.update();

        //scene.add(boxHelper);
    //

    groundBody.collisionFilterGroup = GROUND_GROUP;
    groundBody.collisionFilterMask = PLAYER_GROUP;

    //cameraBody.material = playerMaterial;

    world.addContactMaterial(contactMaterial);
    world.addBody(groundBody);
    
    return { renderGround: cubes, center: center, groundBody: groundBody, groundLimits: groundLimits };
}