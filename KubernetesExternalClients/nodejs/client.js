'use strict';

const { Client } = require('./lib');

const clientConfig = {
    network: {
        clusterMembers: [
            '<EXTERNAL-IP>'
        ]
    }
};

(async () => {
    try {
        const client = await Client.newHazelcastClient(clientConfig);
        const map = await client.getMap('mapForNodejs');
        console.log('Successful connection!');
        console.log('Starting to fill the map with random entries.');
        for (let i = 0; i < 120; i++) {
            const randomKey = Math.floor(Math.random() * 100000);
            try {
                await map.put('key' + randomKey, 'value' + randomKey);
            } catch (error) {
                console.log('Put operation failed: ', error);
            }
            const size = await map.size();
            console.log(`Current map size: ${size}`);
            await new Promise((resolve) => setTimeout(resolve, 1000));
        }
        await client.shutdown();
    } catch (err) {
        console.error('Error occurred:', err);
        process.exit(1);
    }
})();
