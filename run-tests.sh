#!/bin/bash

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
FAILED=0
TOTAL=0
PASSED=0

echo "========================================"
echo "  PopaDin - Executando todos os testes"
echo "========================================"

run_dotnet_test() {
    local name="$1"
    local project="$2"

    echo ""
    echo "  ▸ $name"

    if output=$(dotnet test "$project" --verbosity quiet 2>&1); then
        count=$(echo "$output" | grep -oE 'Aprovado:[[:space:]]*[0-9]+' | grep -oE '[0-9]+' | tail -1)
        count=${count:-$(echo "$output" | grep -oE 'bem-sucedido:[[:space:]]*[0-9]+' | grep -oE '[0-9]+' | tail -1)}
        count=${count:-0}
        PASSED=$((PASSED + count))
        TOTAL=$((TOTAL + count))
        echo "    ✓ $count testes passaram"
    else
        echo "    ✗ FALHOU"
        echo "$output" | tail -5
        FAILED=1
    fi
}

run_jest_test() {
    local name="$1"
    local dir="$2"

    echo ""
    echo "  ▸ $name"

    if output=$(cd "$dir" && npx jest --passWithNoTests 2>&1); then
        count=$(echo "$output" | grep -oE 'Tests:[[:space:]]+[0-9]+ passed' | grep -oE '[0-9]+' | head -1)
        count=${count:-0}
        PASSED=$((PASSED + count))
        TOTAL=$((TOTAL + count))
        echo "    ✓ $count testes passaram"
    else
        echo "    ✗ FALHOU"
        echo "$output" | grep -E "Tests:|FAIL" | head -5
        FAILED=1
    fi
}

# ── .NET Tests ──
echo ""
echo "── .NET Tests ──"

run_dotnet_test "Backend Services" "$ROOT_DIR/Backend/tests/PopaDin.Bkd.Service.Tests/PopaDin.Bkd.Service.Tests.csproj"
run_dotnet_test "Backend API" "$ROOT_DIR/Backend/tests/PopaDin.Bkd.Api.Tests/PopaDin.Bkd.Api.Tests.csproj"
run_dotnet_test "AlertService" "$ROOT_DIR/PopaDin.AlertService/PopaDin.AlertService.Tests/PopaDin.AlertService.Tests.csproj"
run_dotnet_test "ExportService" "$ROOT_DIR/PopaDin.ExportService/PopaDin.ExportService.Tests/PopaDin.ExportService.Tests.csproj"

# ── NestJS Tests ──
echo ""
echo "── NestJS Tests ──"

run_jest_test "NotificationHub" "$ROOT_DIR/PopaDin.NotificationHub"
run_jest_test "AnalyticsService" "$ROOT_DIR/PopaDin.AnalyticsService"

# ── Frontend Tests (Vitest) ──
echo ""
echo "── Frontend Tests ──"

run_vitest_test() {
    local name="$1"
    local dir="$2"

    echo ""
    echo "  ▸ $name"

    if output=$(cd "$dir" && npx vitest run 2>&1); then
        clean=$(echo "$output" | sed 's/\x1b\[[0-9;]*m//g')
        count=$(echo "$clean" | grep 'Tests' | grep 'passed' | grep -oE '[0-9]+ passed' | grep -oE '[0-9]+' | head -1)
        count=${count:-0}
        PASSED=$((PASSED + count))
        TOTAL=$((TOTAL + count))
        echo "    ✓ $count testes passaram"
    else
        echo "    ✗ FALHOU"
        echo "$output" | grep -E "Tests|FAIL|failed" | head -5
        FAILED=1
    fi
}

run_vitest_test "Frontend (React)" "$ROOT_DIR/Frontend"

# ── Resultado ──
echo ""
echo "========================================"
if [ $FAILED -eq 0 ]; then
    echo "  ✓ TODOS OS $TOTAL TESTES PASSARAM"
else
    echo "  ✗ ALGUNS TESTES FALHARAM ($PASSED passaram)"
fi
echo "========================================"

exit $FAILED
