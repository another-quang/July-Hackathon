// Modernised ABR report generation — typed, functional, tested
// Refactored from legacy-abn-report.js preserving exact output

const STATES = ['NSW', 'VIC', 'QLD', 'SA', 'WA', 'TAS', 'NT', 'ACT'] as const;
type State = typeof STATES[number];

interface BusinessRecord {
  name: string;
  state: State | string;
  status: string;
  gst: unknown;
}

interface Summary {
  total: number;
  active: number;
  cancelled: number;
  activePercentage: number;
  gstRegistered: number;
  byState: Record<State, number>;
}

/**
 * Check if a business is registered for GST.
 * Preserves legacy loose-equality behaviour exactly.
 */
function isGstRegistered(gst: unknown): boolean {
  // Deliberately uses loose equality to preserve legacy semantics.
  // eslint-disable-next-line eqeqeq
  return gst == true || gst === 'Y' || gst === 'yes';
}

/**
 * Initialize state counts for all Australian states.
 */
function initializeStateCounts(): Record<State, number> {
  return STATES.reduce((accumulator, state) => {
    accumulator[state] = 0;
    return accumulator;
  }, {} as Record<State, number>);
}

/**
 * Calculate the active percentage, rounded to nearest integer.
 */
function calculateActivePercentage(active: number, total: number): number {
  if (total === 0) return 0;
  return Math.round((active / total) * 100);
}

/**
 * Accumulate summary statistics from business records.
 * Pure function using reduce for immutable computation.
 */
function calculateSummary(records: BusinessRecord[]): Summary {
  const initial = {
    total: 0,
    active: 0,
    cancelled: 0,
    gstRegistered: 0,
    byState: initializeStateCounts(),
  };

  const stats = records.reduce((acc, record) => {
    return {
      total: acc.total + 1,
      active: acc.active + (record.status === 'Active' ? 1 : 0),
      cancelled: acc.cancelled + (record.status === 'Active' ? 0 : 1),
      gstRegistered: acc.gstRegistered + (isGstRegistered(record.gst) ? 1 : 0),
      byState:
        record.state in acc.byState
          ? {
              ...acc.byState,
              [record.state as State]: acc.byState[record.state as State] + 1,
            }
          : acc.byState,
    };
  }, initial);

  return {
    ...stats,
    activePercentage: calculateActivePercentage(stats.active, stats.total),
  };
}

/**
 * Format the summary into the legacy ABR SUMMARY REPORT format.
 * Returns exact same output as legacy version for all valid inputs.
 */
function formatReport(summary: Summary): string {
  const lines = [
    'ABR SUMMARY REPORT',
    '==================',
    `Total businesses: ${summary.total}`,
    `Active: ${summary.active}`,
    `Cancelled: ${summary.cancelled}`,
    `Active percentage: ${summary.activePercentage}%`,
    `GST registered: ${summary.gstRegistered}`,
    'By state:',
  ];

  // Add state counts for non-zero states, in the order of STATES array
  for (const state of STATES) {
    const count = summary.byState[state];
    if (count > 0) {
      lines.push(`  ${state}: ${count}`);
    }
  }

  return lines.join('\n') + '\n';
}

/**
 * Build a summary report from business records.
 * Main entry point — composes calculateSummary and formatReport.
 */
function buildReport(records: BusinessRecord[]): string {
  const summary = calculateSummary(records);
  return formatReport(summary);
}

// Sample data for demonstration
const SAMPLE: BusinessRecord[] = [
  { name: 'Alpha Pty Ltd', state: 'NSW', status: 'Active', gst: true },
  { name: 'Beta Co', state: 'VIC', status: 'Active', gst: 'Y' },
  { name: 'Gamma Group', state: 'VIC', status: 'Cancelled', gst: false },
  { name: 'Delta Trust', state: 'QLD', status: 'Active', gst: 'yes' },
  { name: 'Epsilon Inc', state: 'WA', status: 'Cancelled', gst: 'N' },
];

// Demo: show the refactored version produces the same output
if (typeof require !== 'undefined' && require.main === module) {
  // eslint-disable-next-line no-console
  console.log(buildReport(SAMPLE));
}

// Export for tests
if (typeof module !== 'undefined') {
  module.exports = {
    buildReport,
    calculateSummary,
    formatReport,
    isGstRegistered,
    initializeStateCounts,
    calculateActivePercentage,
    SAMPLE,
    STATES,
  };
}
