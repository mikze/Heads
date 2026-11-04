/** @type {import('next').NextConfig} */
const nextConfig = {
    // Statyczny eksport do out/ — serwer gry (GlowkiServer) serwuje te pliki sam.
    output: 'export',
    images: { unoptimized: true },
    reactStrictMode: true,
};

export default nextConfig;
