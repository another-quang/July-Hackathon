// Characterisation test: Captures CURRENT behaviour of legacy-abn-report.js
// This proves the refactored version produces identical output.

const { buildReport, SAMPLE } = require('./legacy-abn-report');

describe('legacy-abn-report — characterisation tests', () => {
  test('SAMPLE data produces expected report (current behaviour baseline)', () => {
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

  describe('edge cases', () => {
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

    test('GST with various formats (true, "Y", "yes")', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Active', gst: true },
        { name: 'B', state: 'VIC', status: 'Active', gst: 'Y' },
        { name: 'C', state: 'QLD', status: 'Active', gst: 'yes' },
        { name: 'D', state: 'SA', status: 'Active', gst: false },
      ];
      const result = buildReport(records);
      expect(result).toContain('GST registered: 3');
    });

    test('unknown state is ignored', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Active', gst: true },
        { name: 'B', state: 'UNKNOWN', status: 'Active', gst: false },
      ];
      const result = buildReport(records);
      expect(result).toContain('NSW: 1');
      expect(result).not.toContain('UNKNOWN');
    });

    test('single record', () => {
      const records = [
        { name: 'Solo', state: 'WA', status: 'Active', gst: true },
      ];
      const result = buildReport(records);
      expect(result).toContain('Total businesses: 1');
      expect(result).toContain('Active: 1');
      expect(result).toContain('Active percentage: 100%');
      expect(result).toContain('GST registered: 1');
      expect(result).toContain('WA: 1');
    });

    test('percentage rounding (e.g., 66.67% rounds to 67%)', () => {
      const records = [
        { name: 'A', state: 'NSW', status: 'Active', gst: false },
        { name: 'B', state: 'VIC', status: 'Active', gst: false },
        { name: 'C', state: 'QLD', status: 'Cancelled', gst: false },
      ];
      const result = buildReport(records);
      // 2/3 = 0.666... * 100 = 66.666... rounds to 67
      expect(result).toContain('Active percentage: 67%');
    });
  });
});
