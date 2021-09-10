'use strict';

const { Client } = require('./lib');

const clientConfig = {
    network: {
        clusterMembers: [
            <EXTERNAL-IP>
        ]
    }
};

(async () => {
    try {
        const client = await Client.newHazelcastClient(clientConfig);
        const map = await client.getMap('mapForNodejs');
        await map.put('key', 'value');
        const res = await map.get('key');
        if (res !== 'value') {
            throw new Error('Connection failed, check your configuration.');
        }
        console.log('Successful connection!');
        console.log('Starting to fill the map with random entries.');
        let numberOfLoop = 0;
        let count = 120;
        while (numberOfLoop < count) {
            const randomKey = Math.floor(Math.random() * 100000);
            await map.put('key' + randomKey, 'value' + randomKey);
            const size = await map.size();
            console.log(`Current map size: ${size}`);
            await new Promise((resolve) => setTimeout(resolve, 1000));
            numberOfLoop++;
        }
        await client.shutdown();
    } catch (err) {
        console.error('Error occurred:', err);
    }
})();
