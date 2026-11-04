import Head from 'next/head';
import dynamic from 'next/dynamic';

// Cała aplikacja działa w przeglądarce (połączenie SignalR, Phaser), bez renderowania na serwerze.
const App = dynamic(() => import('@/App'), { ssr: false });

export default function Home() {
    return (
        <>
            <Head>
                <title>Główki</title>
                <meta name="description" content="Piłka nożna głowami dla dwóch graczy" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
            </Head>
            <App />
        </>
    );
}
