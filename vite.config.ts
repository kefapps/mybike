import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

const VENDOR_CHUNK_WARNING_LIMIT_KB = 380;

export default defineConfig({
  plugins: [react()],
  build: {
    chunkSizeWarningLimit: VENDOR_CHUNK_WARNING_LIMIT_KB,
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (/[\\/]node_modules[\\/](react|react-dom|scheduler)[\\/]/.test(id)) {
            return "vendor-react";
          }

          if (/[\\/]node_modules[\\/]three[\\/]build[\\/]/.test(id)) {
            return "vendor-three-build";
          }

          if (/[\\/]node_modules[\\/]three[\\/]/.test(id)) {
            const sourcePathMatch = id.match(
              /[\\/]node_modules[\\/]three[\\/]src[\\/]([^/\\\\]+)[\\/]/
            );

            if (sourcePathMatch?.[1]) {
              return `vendor-three-${sourcePathMatch[1]}`;
            }

            return "vendor-three";
          }

          if (id.includes("node_modules")) {
            return "vendor";
          }
        }
      }
    }
  },
  test: {
    globals: true,
    environment: "node",
    exclude: ["**/node_modules/**", "**/dist/**"]
  }
});
