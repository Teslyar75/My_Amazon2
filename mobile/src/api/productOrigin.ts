import { Platform } from "react-native";
import Constants from "expo-constants";

const CONFIGURED = (process.env.EXPO_PUBLIC_PRODUCT_URL || "").replace(/\/$/, "");
const DEFAULT_PORT = "5272";

function isLoopbackHost(host: string): boolean {
  const h = host.toLowerCase();
  return h === "localhost" || h === "127.0.0.1" || h === "::1";
}

function isLoopbackUrl(url: string): boolean {
  try {
    return isLoopbackHost(new URL(url).hostname);
  } catch {
    return /localhost|127\.0\.0\.1/i.test(url);
  }
}

/** Host машины с Metro (Expo Go), например 192.168.0.105 — не телефон. */
export function expoDevMachineHost(): string | null {
  const anyConst = Constants as {
    expoConfig?: { hostUri?: string };
    manifest2?: { extra?: { expoClient?: { hostUri?: string } } };
    manifest?: { debuggerHost?: string };
  };
  const hostUri =
    anyConst.expoConfig?.hostUri ||
    anyConst.manifest2?.extra?.expoClient?.hostUri ||
    anyConst.manifest?.debuggerHost ||
    "";
  if (!hostUri) return null;
  const hostPort = hostUri.replace(/^[a-z]+:\/\//i, "").split("/")[0] || "";
  const host = hostPort.split(":")[0]?.trim();
  if (!host || isLoopbackHost(host)) return null;
  return host;
}

function portFromUrl(url: string): string {
  try {
    return new URL(url).port || DEFAULT_PORT;
  } catch {
    const m = url.match(/:(\d+)/);
    return m?.[1] || DEFAULT_PORT;
  }
}

/**
 * Origin Product API для native (Expo Go / эмулятор).
 * Web не использует — там Metro proxy `/api`.
 */
export function resolveNativeProductOrigin(): string {
  const configured = CONFIGURED || `http://localhost:${DEFAULT_PORT}`;

  if (!isLoopbackUrl(configured)) return configured;

  const lanHost = expoDevMachineHost();
  if (lanHost) {
    return `http://${lanHost}:${portFromUrl(configured)}`;
  }

  // Android emulator → хост ПК
  if (Platform.OS === "android") {
    return `http://10.0.2.2:${portFromUrl(configured)}`;
  }

  return configured;
}

export function getConfiguredProductUrl(): string {
  return CONFIGURED;
}
