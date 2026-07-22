// Refactored version tests — verify behaviour preservation
// These tests validate that the TypeScript refactor produces identical output to legacy

// Note: This test file would normally be compiled from .ts to .js
// For simplicity, we show the intended test structure
// In practice, you'd run: npx ts-jest or build with tsc then test

const {
  buildReport,
  calculateSummary,
  formatReport,
  isGstRegistered,
  initializeStateCounts,
  calculateActivePercentage,
  SAMPLE,
} = require('./legacy-abn-report-refactored.ts');

describe('legacy-abn-report-refactored — behaviour preservation', () => {
  test('SAMPLE data produces identical report to legacy version', () => {
    const result = buildReport(SAMPLE);
    const expected = `ABR SUMMARY REPORT
==================
Total businesses: 5
Active: 3
Cancelled: 2
Active percentage: 60%
GST registered: 3
By state:
  NSW: 1
  VIC: 2
  QLD: 1
  WA: 1
`;
    expect(result).toBe(expected);
  });

  describe('unit tests for pure functions', () => {
    describe('isGstRegistered', () => {
      test('returns true for boolean true', () => {
        expect(isGstRegistered(true)).toBe(true);
      });

      test('returns true for string "Y"', () => {
        expect(isGstRegistered('Y')).toBe(true);
      });

      test('returns true for string "yes"', () => {
        expect(isGstRegistered('yes')).toBe(true);
      });

      test('returns false for boolean false', () => {
        expect(isGstRegistered(false)).toBe(false);
      });

      test('returns false for string "N"', () => {
        expect(isGstRegistered('N')).toBe(false);
      });

      test('returns false for empty string', () => {
        expect(isGstRegistered('')).toBe(false);
      });
    });

    describe('initializeStateCounts', () => {
      test('creates object with all states initialized to 0', () => {
        const counts = initializeStateCounts();
        expect(counts).toEqual({
          NSW: 0,
          VIC: 0,
          QLD: 0,
          SA: 0,
          WA: 0,
          TAS: 0,
          NT: 0,
          ACT: 0,
        });
      });
    });

    describe('calculateActivePercentage', () => {
      test('returns 0 for empty records', () => {
        expect(calculateActivePercentage(0, 0)).toBe(0);
      });

      test('returns 100 for all active', () => {
        expect(calculateActivePercentage(5, 5)).toBe(100);
      });

      test('returns 0 for no active', () => {
        expect(calculateActivePercentage(0, 5)).toBe(0);
      });

      test('rounds correctly (60%)', () => {
        expect(calculateActivePercentage(3, 5)).toBe(60);
      });

      test('rounds correctly (66.67% → 67%)', () => {
        expect(calculateActivePercentage(2, 3)).toBe(67);
      });
    });

    describe('calculateSummary', () => {
      test('empty records array returns zeros', () => {
        const summary = calculateSummary([]);
        expect(summary.total).toBe(0);
        expect(summary.active).toBe(0);
        expect(summary.cancelled).toBe(0);
        expect(summary.gstRegistered).toBe(0);
      });

      test('SAMPLE data produces correct summary', () => {
        const summary = calculateSummary(SAMPLE);
        expect(summary.total).toBe(5);
        expect(summary.active).toBe(3);
        expect(summary.cancelled).toBe(2);
        expect(summary.activePercentage).toBe(60);
        expect(summary.gstRegistered).toBe(3);
        expect(summary.byState).toEqual({
          NSW: 1,
          VIC: 2,
          QLD: 1,
          SA: 0,
          WA: 1,
          TAS: 0,
          NT: 0,
          ACT: 0,
        });
      });
    });

    describe('formatReport', () => {
      test('produces correctly formatted report from summary', () => {
        const summary = calculateSummary(SAMPLE);
        const report = formatReport(summary);
        expect(report).toContain('ABR SUMMARY REPORT');
        expect(report).toContain('==================');
        expect(report).toContain('Total businesses: 5');
        expect(report).toContain('Active: 3');
        expect(report).toContain('Cancelled: 2');
        expect(report).toContain('Active percentage: 60%');
        expect(report).toContain('GST registered: 3');
        expect(report).toContain('By state:');
        expect(report).toContain('  NSW: 1');
      });
    });
  });

  describe('edge cases (refactored version)', () => {
    test('empty records array', () => {
      const result = buildReport([]);
      expect(result).toContain('Total businesses: 0');
      expect(result).toContain('Active: 0');
      expect(result).toContain('Cancelled: 0');
      expect(result).toContain('Active percentage: 0%');
      expect(result).toContain('GST registered: 0');
    });

    test('all active records', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Active', gst: true },
        { name: 'B', state: 'VIC', status: 'Active', gst: 'Y' },
      ];
      const result = buildReport(records);
      expect(result).toContain('Total businesses: 2');
      expect(result).toContain('Active: 2');
      expect(result).toContain('Cancelled: 0');
      expect(result).toContain('Active percentage: 100%');
    });

    test('all cancelled records', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Cancelled', gst: false },
        { name: 'B', state: 'VIC', status: 'Cancelled', gst: 'N' },
      ];
      const result = buildReport(records);
      expect(result).toContain('Total businesses: 2');
      expect(result).toContain('Active: 0');
      expect(result).toContain('Cancelled: 2');
      expect(result).toContain('Active percentage: 0%');
    });

    test('no GST registered', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Active', gst: false },
        { name: 'B', state: 'VIC', status: 'Active', gst: 'N' },
      ];
      const result = buildReport(records);
      expect(result).toContain('GST registered: 0');
    });

    test('GST with various formats', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Active', gst: true },
        { name: 'B', state: 'VIC', status: 'Active', gst: 'Y' },
        { name: 'C', state: 'QLD', status: 'Active', gst: 'yes' },
        { name: 'D', state: 'SA', status: 'Active', gst: false },
      ];
      const result = buildReport(records);
      expect(result).toContain('GST registered: 3');
    });

    test('single record', () => {
      const records = [{ name: 'Solo', state: 'WA', status: 'Active', gst: true }];
      const result = buildReport(records);
      expect(result).toContain('Total businesses: 1');
      expect(result).toContain('Active: 1');
      expect(result).toContain('Active percentage: 100%');
      expect(result).toContain('GST registered: 1');
      expect(result).toContain('WA: 1');
    });

    test('percentage rounding', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Active', gst: false },
        { name: 'B', state: 'VIC', status: 'Active', gst: false },
        { name: 'C', state: 'QLD', status: 'Cancelled', gst: false },
      ];
      const result = buildReport(records);
      expect(result).toContain('Active percentage: 67%');
    });

    test('unknown state is ignored in by-state output (legacy behaviour)', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Active', gst: true },
        { name: 'B', state: 'UNKNOWN', status: 'Active', gst: false },
      ];
      const result = buildReport(records);
      expect(result).toContain('NSW: 1');
      expect(result).not.toContain('UNKNOWN');
    });

    test('non-Active status counts as cancelled (legacy behaviour)', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Pending', gst: false },
      ];
      const result = buildReport(records);
      expect(result).toContain('Total businesses: 1');
      expect(result).toContain('Active: 0');
      expect(result).toContain('Cancelled: 1');
      expect(result).toContain('Active percentage: 0%');
    });

    test('GST values loosely equal to true are counted (legacy behaviour)', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Active', gst: 1 },
        { name: 'B', state: 'VIC', status: 'Active', gst: '1' },
        { name: 'C', state: 'QLD', status: 'Active', gst: true },
      ];
      const result = buildReport(records);
      expect(result).toContain('GST registered: 3');
    });
  });
});
